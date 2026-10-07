using Microsoft.EntityFrameworkCore;
using Recommenda.API.ExceptionHandling;
using Recommenda.Application.Interfaces.Repositories;
using Recommenda.Application.Services;
using Recommenda.Infrastructure.Persistence;
using Recommenda.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<MySqlDuplicateKeyExceptionHandler>();
builder.Services.AddScoped<GenreService>();

var connectionString = builder.Configuration.GetConnectionString("MySql")
    ?? throw new InvalidOperationException("Connection string 'MySql' não foi encontrada.");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Configure ConnectionStrings:MySql para o ambiente atual.");

builder.Services.AddDbContext<RecommendaContext>(options =>
{
    options.UseMySQL(connectionString);
}, contextLifetime: ServiceLifetime.Scoped, optionsLifetime: ServiceLifetime.Scoped);

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.MapGet("/health", async (RecommendaContext context, CancellationToken cancellationToken) =>
{
    var connected = await context.Database.CanConnectAsync(cancellationToken);
    return connected
        ? (IResult)Results.Ok(new { status = "healthy", database = "MySQL" })
        : Results.Problem(
            statusCode: StatusCodes.Status503ServiceUnavailable,
            title: "Banco de dados indisponível.",
            detail: "Verifique o MySQL e a configuração ConnectionStrings:MySql.");
});

app.Run();
