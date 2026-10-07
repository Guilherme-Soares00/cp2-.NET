using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Recommenda.Infrastructure.Persistence;
using Xunit;

namespace Recommenda.IntegrationTests;

[CollectionDefinition(MySqlCollection.Name, DisableParallelization = true)]
public sealed class MySqlCollection : ICollectionFixture<MySqlFixture>
{
    public const string Name = "MySQL real";
}

public sealed class MySqlFixture : IAsyncLifetime
{
    private const string SchemaPrefix = "RecommendaCp2Tests_";
    private readonly string _schemaName = SchemaPrefix + Guid.NewGuid().ToString("N");
    private string? _adminConnectionString;
    private string? _testConnectionString;
    private bool _schemaCreated;

    public async Task InitializeAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable("RECOMMENDA_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Defina RECOMMENDA_TEST_CONNECTION para o MySQL de desenvolvimento antes de executar os testes. " +
                "Exemplo: Server=127.0.0.1;Port=3306;User=root;Password=TDSPB123;. " +
                "Os testes criam e removem apenas um schema temporário exclusivo.");

        var builder = new MySqlConnectionStringBuilder(connectionString) { Database = string.Empty };
        _adminConnectionString = builder.ConnectionString;
        builder.Database = _schemaName;
        _testConnectionString = builder.ConnectionString;

        await using var context = CreateContext();
        var migrations = context.Database.GetMigrations().ToArray();
        if (migrations.Length != 1 || !migrations[0].EndsWith("_InitialCreate", StringComparison.Ordinal))
            throw new InvalidOperationException(
                "Os testes esperam uma única migration InitialCreate na Infrastructure.");

        await using (var connection = new MySqlConnection(_adminConnectionString))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = $"CREATE DATABASE `{_schemaName}` CHARACTER SET utf8mb4";
            await command.ExecuteNonQueryAsync();
            _schemaCreated = true;
        }

        try
        {
            await context.Database.MigrateAsync();
            var applied = (await context.Database.GetAppliedMigrationsAsync()).ToArray();
            if (!applied.SequenceEqual(migrations))
                throw new InvalidOperationException("A migration InitialCreate não foi aplicada no schema temporário.");
        }
        catch
        {
            await DropTemporarySchemaAsync();
            throw;
        }
    }

    public RecommendaContext CreateContext()
    {
        if (_testConnectionString is null)
            throw new InvalidOperationException("A fixture MySQL ainda não foi inicializada.");

        var options = new DbContextOptionsBuilder<RecommendaContext>()
            .UseMySQL(_testConnectionString)
            .Options;
        return new RecommendaContext(options);
    }

    public Task DisposeAsync() => DropTemporarySchemaAsync();

    private async Task DropTemporarySchemaAsync()
    {
        if (!_schemaCreated || _adminConnectionString is null)
            return;

        // O identificador é gerado internamente; nunca usamos Database recebido do ambiente.
        if (!_schemaName.StartsWith(SchemaPrefix, StringComparison.Ordinal) ||
            !Guid.TryParseExact(_schemaName[SchemaPrefix.Length..], "N", out _))
            throw new InvalidOperationException("Nome de schema temporário inválido; limpeza cancelada.");

        await using var connection = new MySqlConnection(_adminConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"DROP DATABASE IF EXISTS `{_schemaName}`";
        await command.ExecuteNonQueryAsync();
        _schemaCreated = false;
    }
}
