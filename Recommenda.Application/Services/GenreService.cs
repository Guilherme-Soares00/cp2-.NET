using Recommenda.Application.Interfaces.Repositories;
using Recommenda.Application.DTOs.Genres;
using Recommenda.Domain.Entities;

namespace Recommenda.Application.Services;

public sealed class GenreService(IRepository<Genre> genreRepository)
{
    public async Task<IReadOnlyList<GenreResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var genres = await genreRepository.GetAllAsync(cancellationToken);
        return genres.Select(ToResponse).ToList();
    }

    public async Task<GenreResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var genre = await genreRepository.GetByIdAsync(id, cancellationToken);
        return genre is null ? null : ToResponse(genre);
    }

    public async Task<GenreResponse> CreateAsync(GenreRequest request, CancellationToken cancellationToken = default)
    {
        var genre = new Genre(request.Name, request.Description);
        await genreRepository.AddAsync(genre, cancellationToken);
        return ToResponse(genre);
    }

    public async Task<GenreResponse?> UpdateAsync(
        Guid id, GenreRequest request, CancellationToken cancellationToken = default)
    {
        var genre = await genreRepository.GetByIdAsync(id, cancellationToken);
        if (genre is null)
            return null;

        genre.UpdateDetails(request.Name, request.Description);
        var updatedGenre = await genreRepository.UpdateAsync(genre, cancellationToken);
        return updatedGenre is null ? null : ToResponse(updatedGenre);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        genreRepository.DeleteAsync(id, cancellationToken);

    private static GenreResponse ToResponse(Genre genre) =>
        new(genre.Id, genre.Name, genre.Description, genre.CreatedAt, genre.UpdateAt);
}
