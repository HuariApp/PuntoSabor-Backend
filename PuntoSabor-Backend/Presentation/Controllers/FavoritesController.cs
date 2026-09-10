using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using PuntoSabor_Backend.Favorites.Domain.Model;
using PuntoSabor_Backend.Favorites.Domain.Repositories;
using PuntoSabor_Backend.Presentation.Resources;
using PuntoSabor_Backend.Shared.Domain.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace PuntoSabor_Backend.Presentation.Controllers;

[ApiController]
[Route("favorites")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Favorites Endpoints.")]
public class FavoritesController(
    IFavoriteRepository favorites,
    IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation("Get Favorites", "Returns all favorite huariques for a user.", OperationId = "GetFavorites")]
    [SwaggerResponse(200, "Lista de favoritos.", typeof(IEnumerable<FavoriteResource>))]
    [SwaggerResponse(400, "Parámetro inválido.", typeof(ErrorResource))]
    public async Task<IActionResult> GetByUser([FromQuery] int userId, CancellationToken ct)
    {
        if (userId <= 0)
            return BadRequest(new ErrorResource("El parámetro 'userId' debe ser mayor a 0."));

        var result = await favorites.FindByUserIdAsync(userId, ct);
        var resources = result.Select(f => new FavoriteResource(f.Id, f.UserId, f.HuariqueId, f.CreatedAt));
        return Ok(resources);
    }

    [HttpPost("{huariqueId:int}")]
    [SwaggerOperation("Add Favorite", "Marks a huarique as favorite for a user.", OperationId = "AddFavorite")]
    [SwaggerResponse(201, "Favorito agregado.", typeof(FavoriteResource))]
    [SwaggerResponse(400, "Parámetros inválidos.", typeof(ErrorResource))]
    [SwaggerResponse(409, "El huarique ya está en favoritos.", typeof(ErrorResource))]
    public async Task<IActionResult> Add(int huariqueId, [FromQuery] int userId, CancellationToken ct)
    {
        if (userId <= 0)
            return BadRequest(new ErrorResource("El parámetro 'userId' debe ser mayor a 0."));

        if (huariqueId <= 0)
            return BadRequest(new ErrorResource("El parámetro 'huariqueId' debe ser mayor a 0."));

        var existing = await favorites.FindByUserAndHuariqueAsync(userId, huariqueId, ct);
        if (existing is not null)
            return Conflict(new ErrorResource("El huarique ya está en favoritos."));

        var favorite = new Favorite { UserId = userId, HuariqueId = huariqueId };
        await favorites.AddAsync(favorite, ct);
        await unitOfWork.CompleteAsync(ct);

        return CreatedAtAction(nameof(GetByUser), new { userId },
            new FavoriteResource(favorite.Id, favorite.UserId, favorite.HuariqueId, favorite.CreatedAt));
    }

    [HttpDelete("{huariqueId:int}")]
    [SwaggerOperation("Remove Favorite", "Removes a huarique from a user's favorites.", OperationId = "RemoveFavorite")]
    [SwaggerResponse(204, "Favorito eliminado.")]
    [SwaggerResponse(400, "Parámetros inválidos.", typeof(ErrorResource))]
    [SwaggerResponse(404, "El favorito no existe.", typeof(ErrorResource))]
    public async Task<IActionResult> Remove(int huariqueId, [FromQuery] int userId, CancellationToken ct)
    {
        if (userId <= 0)
            return BadRequest(new ErrorResource("El parámetro 'userId' debe ser mayor a 0."));

        var favorite = await favorites.FindByUserAndHuariqueAsync(userId, huariqueId, ct);
        if (favorite is null)
            return NotFound(new ErrorResource("El favorito no existe."));

        favorites.Remove(favorite);
        await unitOfWork.CompleteAsync(ct);
        return NoContent();
    }

     // Helper reservado para validación futura de favoritos duplicados.
    private static bool CanAddFavorite(int userId, int huariqueId) =>
        userId > 0 && huariqueId > 0;
}
