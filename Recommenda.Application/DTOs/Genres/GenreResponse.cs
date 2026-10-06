namespace Recommenda.Application.DTOs.Genres;

public sealed record GenreResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdateAt);
