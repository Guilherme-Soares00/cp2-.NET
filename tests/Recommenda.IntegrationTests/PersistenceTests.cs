using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Recommenda.Domain.Entities;
using Recommenda.Infrastructure.Persistence;
using Recommenda.Infrastructure.Persistence.Repositories;
using Xunit;

namespace Recommenda.IntegrationTests;

[Collection(MySqlCollection.Name)]
public sealed class PersistenceTests(MySqlFixture fixture)
{
    [Fact]
    public Task Complete_catalog_roundtrips_with_TPH_and_many_to_many_relationships() =>
        InTransactionAsync(async context =>
        {
            var scienceFiction = new Genre("Ficção científica", "Viagens no espaço");
            var drama = new Genre("Drama", "Histórias e conflitos");
            var movie = CreateMovie();
            movie.Genres.Add(scienceFiction);
            var serie = CreateSerie();
            serie.Genres.AddRange([scienceFiction, drama]);
            var season = CreateSeason(1);
            season.ReleaseDate = new DateOnly(2025, 3, 1);
            var episode = CreateEpisode(1);
            season.Episodes.Add(episode);
            serie.Seasons.Add(season);
            var user = CreateUser();
            user.Configuration = new UserConfiguration { Theme = "Dark", EnableNotifications = true };
            user.Ratings.Add(new Rating { Content = movie, Score = 5 });
            context.AddRange(movie, serie, user);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var storedMovie = Assert.IsType<Movie>(await context.Contents
                .Include(content => content.Genres)
                .Include(content => content.Ratings)
                .SingleAsync(content => content.Id == movie.Id));
            Assert.Equal(movie.Title, storedMovie.Title);
            Assert.Equal(movie.Description, storedMovie.Description);
            Assert.Equal(movie.Company, storedMovie.Company);
            Assert.Equal(movie.ReleaseDate, storedMovie.ReleaseDate);
            Assert.Equal(120, storedMovie.DurationInMinutes);
            Assert.Equal(scienceFiction.Id, Assert.Single(storedMovie.Genres).Id);
            Assert.Equal(5, Assert.Single(storedMovie.Ratings).Score);

            var storedSerie = await context.Series.Include(content => content.Genres)
                .Include(content => content.Seasons).ThenInclude(item => item.Episodes)
                .SingleAsync(content => content.Id == serie.Id);
            Assert.True(storedSerie.IsEnded);
            Assert.Equal(2, storedSerie.Genres.Count);
            var storedSeason = Assert.Single(storedSerie.Seasons);
            Assert.Equal(serie.Id, storedSeason.SerieId);
            Assert.Equal(season.ReleaseDate, storedSeason.ReleaseDate);
            var storedEpisode = Assert.Single(storedSeason.Episodes);
            Assert.Equal(season.Id, storedEpisode.SeasonId);
            Assert.Equal(episode.ReleaseDate, storedEpisode.ReleaseDate);
            Assert.Equal(45, storedEpisode.DurationInMinutes);

            var storedUser = await context.Users.Include(item => item.Configuration)
                .Include(item => item.Ratings).SingleAsync(item => item.Id == user.Id);
            Assert.NotNull(storedUser.Configuration);
            Assert.Equal(user.Id, storedUser.Configuration.UserId);
            Assert.Equal("Dark", storedUser.Configuration.Theme);
            Assert.True(storedUser.Configuration.EnableNotifications);
            var storedRating = Assert.Single(storedUser.Ratings);
            Assert.Equal(movie.Id, storedRating.ContentId);
            Assert.Equal(user.Id, storedRating.UserId);

            var inverseGenre = await context.Genres.Include(item => item.Contents)
                .SingleAsync(item => item.Id == scienceFiction.Id);
            Assert.Equal(2, inverseGenre.Contents.Count);
            Assert.Contains(inverseGenre.Contents, item => item is Movie);
            Assert.Contains(inverseGenre.Contents, item => item is Serie);
            Assert.Equal(3, await context.Set<Dictionary<string, object>>("ContentGenre").CountAsync());
            var discriminators = await context.Contents
                .Select(item => new { item.Id, Type = EF.Property<string>(item, "ContentType") })
                .ToDictionaryAsync(item => item.Id, item => item.Type);
            Assert.Equal("Movie", discriminators[movie.Id]);
            Assert.Equal("Serie", discriminators[serie.Id]);
        });

    [Fact]
    public Task Optional_fields_and_user_without_configuration_are_persisted() =>
        InTransactionAsync(async context =>
        {
            var genre = new Genre("Documentário");
            var user = CreateUser();
            var serie = CreateSerie();
            var firstSeason = CreateSeason(1);
            var secondSeason = CreateSeason(2);
            firstSeason.Episodes.Add(CreateEpisode(1));
            secondSeason.Episodes.Add(CreateEpisode(1));
            serie.Seasons.AddRange([firstSeason, secondSeason]);
            context.AddRange(genre, user, serie);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            Assert.Null((await context.Genres.SingleAsync(item => item.Id == genre.Id)).Description);
            Assert.Null((await context.Users.Include(item => item.Configuration)
                .SingleAsync(item => item.Id == user.Id)).Configuration);
            Assert.All(await context.Seasons.ToListAsync(), item => Assert.Null(item.ReleaseDate));
            // O episódio 1 pode se repetir em temporadas diferentes.
            Assert.Equal(2, await context.Episodes.CountAsync(item => item.Number == 1));
        });

    [Theory]
    [InlineData("email")]
    [InlineData("user-configuration")]
    [InlineData("episode-number")]
    public Task Unique_constraints_reject_duplicate_values(string constraint) =>
        InTransactionAsync(async context =>
        {
            switch (constraint)
            {
                case "email":
                    var user = CreateUser();
                    context.Users.Add(user);
                    await context.SaveChangesAsync();
                    context.ChangeTracker.Clear();
                    var duplicateUser = CreateUser();
                    duplicateUser.Email = user.Email;
                    context.Users.Add(duplicateUser);
                    break;
                case "user-configuration":
                    var configuredUser = CreateUser();
                    configuredUser.Configuration = new UserConfiguration { Theme = "Dark" };
                    context.Users.Add(configuredUser);
                    await context.SaveChangesAsync();
                    context.ChangeTracker.Clear();
                    context.UserConfigurations.Add(new UserConfiguration
                    {
                        UserId = configuredUser.Id,
                        Theme = "Light"
                    });
                    break;
                case "episode-number":
                    var serie = CreateSerie();
                    var season = CreateSeason(1);
                    season.Episodes.Add(CreateEpisode(1));
                    serie.Seasons.Add(season);
                    context.Series.Add(serie);
                    await context.SaveChangesAsync();
                    context.ChangeTracker.Clear();
                    var duplicateEpisode = CreateEpisode(1);
                    duplicateEpisode.SeasonId = season.Id;
                    context.Episodes.Add(duplicateEpisode);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(constraint));
            }

            var exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
            Assert.Equal(1062, Assert.IsType<MySqlException>(exception.InnerException).Number);
        });

    [Fact]
    public Task Foreign_key_rejects_episode_without_existing_season() =>
        InTransactionAsync(async context =>
        {
            var episode = CreateEpisode(1);
            episode.SeasonId = Guid.NewGuid();
            context.Episodes.Add(episode);

            var exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
            Assert.Equal(1452, Assert.IsType<MySqlException>(exception.InnerException).Number);
        });

    [Fact]
    public Task Rating_score_outside_one_to_five_is_rejected_by_database() =>
        InTransactionAsync(async context =>
        {
            var movie = CreateMovie();
            var user = CreateUser();
            context.AddRange(movie, user);
            await context.SaveChangesAsync();
            context.Ratings.Add(new Rating { ContentId = movie.Id, UserId = user.Id, Score = 6 });

            var exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
            Assert.Equal(3819, Assert.IsType<MySqlException>(exception.InnerException).Number);
        });

    [Fact]
    public Task Database_cascades_remove_dependents_and_preserve_unrelated_entities() =>
        InTransactionAsync(async context =>
        {
            var genre = new Genre("Aventura");
            var serie = CreateSerie();
            serie.Genres.Add(genre);
            var season = CreateSeason(1);
            var episode = CreateEpisode(1);
            season.Episodes.Add(episode);
            serie.Seasons.Add(season);
            var movie = CreateMovie();
            movie.Genres.Add(genre);
            var user = CreateUser();
            user.Configuration = new UserConfiguration { Theme = "Light" };
            user.Ratings.AddRange([
                new Rating { Content = serie, Score = 4 },
                new Rating { Content = movie, Score = 5 }
            ]);
            context.AddRange(serie, movie, user);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            // Carregamos apenas o principal: a remoção dos filhos deve ocorrer no MySQL.
            context.Series.Remove(await context.Series.SingleAsync(item => item.Id == serie.Id));
            await context.SaveChangesAsync();
            Assert.False(await context.Seasons.AnyAsync(item => item.Id == season.Id));
            Assert.False(await context.Episodes.AnyAsync(item => item.Id == episode.Id));
            Assert.False(await context.Ratings.AnyAsync(item => item.ContentId == serie.Id));
            Assert.Equal(1, await context.Set<Dictionary<string, object>>("ContentGenre").CountAsync());
            Assert.True(await context.Movies.AnyAsync(item => item.Id == movie.Id));
            Assert.True(await context.Genres.AnyAsync(item => item.Id == genre.Id));
            Assert.True(await context.UserConfigurations.AnyAsync(item => item.UserId == user.Id));
            context.ChangeTracker.Clear();

            context.Users.Remove(await context.Users.SingleAsync(item => item.Id == user.Id));
            await context.SaveChangesAsync();
            Assert.False(await context.UserConfigurations.AnyAsync(item => item.UserId == user.Id));
            Assert.False(await context.Ratings.AnyAsync(item => item.UserId == user.Id));
            Assert.True(await context.Movies.AnyAsync(item => item.Id == movie.Id));
            Assert.True(await context.Genres.AnyAsync(item => item.Id == genre.Id));
        });

    [Fact]
    public Task Repository_CRUD_handles_detached_and_tracked_updates_and_missing_ids() =>
        InTransactionAsync(async context =>
        {
            var repository = new Repository<Genre>(context);
            var original = await repository.AddAsync(new Genre("Comédia", "Descrição inicial"));
            var detached = Assert.IsType<Genre>(await repository.GetByIdAsync(original.Id));
            Assert.NotSame(original, detached);
            Assert.Equal(EntityState.Detached, context.Entry(detached).State);
            var previousUpdate = detached.UpdateAt;
            detached.UpdateDetails("Comédia dramática", "Descrição atualizada");

            var updated = await repository.UpdateAsync(detached);
            Assert.Same(original, updated);
            Assert.Single(context.ChangeTracker.Entries<Genre>());
            var stored = Assert.IsType<Genre>(await repository.GetByIdAsync(original.Id));
            Assert.Equal("Comédia dramática", stored.Name);
            Assert.Equal("Descrição atualizada", stored.Description);
            Assert.True(stored.UpdateAt >= previousUpdate);
            context.ChangeTracker.Clear();

            var tracked = await context.Genres.SingleAsync(item => item.Id == original.Id);
            tracked.UpdateDetails("Comédia", null);
            Assert.Same(tracked, await repository.UpdateAsync(tracked));
            Assert.Null(Assert.IsType<Genre>(await repository.GetByIdAsync(original.Id)).Description);
            Assert.Single(await repository.GetAllAsync());
            Assert.True(await repository.DeleteAsync(original.Id));
            Assert.Null(await repository.GetByIdAsync(original.Id));
            Assert.False(await repository.DeleteAsync(original.Id));
            Assert.Null(await repository.UpdateAsync(new Genre("Inexistente")));
            Assert.Empty(await repository.GetAllAsync());
        });

    [Fact]
    public Task Repository_forwards_query_cancellation() =>
        InTransactionAsync(async context =>
        {
            var repository = new Repository<Genre>(context);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => repository.GetAllAsync(cancellation.Token));
        });

    private async Task InTransactionAsync(Func<RecommendaContext, Task> test)
    {
        await using var context = fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            await test(context);
        }
        finally
        {
            await transaction.RollbackAsync();
        }
    }

    private static Movie CreateMovie() => new()
    {
        Title = "Viagem ao espaço",
        Description = "Uma missão científica.",
        Company = "Estúdio de testes",
        ReleaseDate = new DateOnly(2025, 1, 1),
        DurationInMinutes = 120
    };

    private static Serie CreateSerie() => new()
    {
        Title = "Histórias do futuro",
        Description = "Uma série de ficção.",
        Company = "Estúdio de testes",
        ReleaseDate = new DateOnly(2025, 2, 1),
        IsEnded = true
    };

    private static Season CreateSeason(int number) => new()
    {
        Number = number,
        Title = $"Temporada {number}",
        Description = "Temporada de testes"
    };

    private static Episode CreateEpisode(int number) => new()
    {
        Number = number,
        Title = $"Episódio {number}",
        DurationInMinutes = 45,
        ReleaseDate = new DateOnly(2025, 3, 10)
    };

    private static User CreateUser() => new()
    {
        Name = "Usuário de teste",
        Email = $"teste-{Guid.NewGuid():N}@example.com"
    };
}
