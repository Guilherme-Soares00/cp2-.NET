using Microsoft.AspNetCore.Mvc;
using Recommenda.Application.DTOs.Genres;
using Recommenda.Application.Services;

namespace Recommenda.API.Controllers;

[ApiController]
[Route("api/genres")]
public sealed class GenresController(GenreService genreService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<GenreResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<GenreResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await genreService.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<GenreResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GenreResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var genre = await genreService.GetByIdAsync(id, cancellationToken);
        return genre is null ? NotFound() : Ok(genre);
    }

    [HttpPost]
    [ProducesResponseType<GenreResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GenreResponse>> Create(
        [FromBody] GenreRequest request, CancellationToken cancellationToken)
    {
        var genre = await genreService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = genre.Id }, genre);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<GenreResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GenreResponse>> Update(
        Guid id, [FromBody] GenreRequest request, CancellationToken cancellationToken)
    {
        var genre = await genreService.UpdateAsync(id, request, cancellationToken);
        return genre is null ? NotFound() : Ok(genre);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        await genreService.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}
