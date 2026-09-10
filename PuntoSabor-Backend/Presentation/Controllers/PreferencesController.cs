using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using PuntoSabor_Backend.Presentation.Resources;
using PuntoSabor_Backend.Shared.Domain.Repositories;
using PuntoSabor_Backend.UserPreferences.Domain.Model;
using PuntoSabor_Backend.UserPreferences.Domain.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace PuntoSabor_Backend.Presentation.Controllers;

/**
 * <summary>
 *     Preferencias del usuario (US17) y configuración de notificaciones (US11).
 * </summary>
 */
[ApiController]
[Route("preferences")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("User Preferences Endpoints.")]
public class PreferencesController(
    IUserPreferenceRepository preferences,
    IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation("Get Preferences", "Returns the saved preferences for a user.", OperationId = "GetPreferences")]
    [SwaggerResponse(200, "Preferencias del usuario.", typeof(UserPreferenceResource))]
    [SwaggerResponse(400, "Parámetro inválido.", typeof(ErrorResource))]
    public async Task<IActionResult> Get([FromQuery] int userId, CancellationToken ct)
    {
        if (userId <= 0)
            return BadRequest(new ErrorResource("El parámetro 'userId' debe ser mayor a 0."));

        var pref = await preferences.FindByUserIdAsync(userId, ct);
        // Si aún no existen, devolvemos los valores por defecto (sin persistir).
        if (pref is null)
            return Ok(new UserPreferenceResource(userId, null, null, null, true));

        return Ok(ToResource(pref));
    }

    [HttpPut]
    [Consumes(MediaTypeNames.Application.Json)]
    [SwaggerOperation("Save Preferences", "Creates or updates the preferences for a user.", OperationId = "SavePreferences")]
    [SwaggerResponse(200, "Preferencias guardadas.", typeof(UserPreferenceResource))]
    [SwaggerResponse(400, "Datos inválidos.", typeof(ErrorResource))]
    public async Task<IActionResult> Save([FromQuery] int userId, [FromBody] UpdatePreferenceResource resource, CancellationToken ct)
    {
        if (userId <= 0)
            return BadRequest(new ErrorResource("El parámetro 'userId' debe ser mayor a 0."));
        if (resource is null)
            return BadRequest(new ErrorResource("Body inválido."));
        if (resource.MaxBudget is < 0)
            return BadRequest(new ErrorResource("El presupuesto no puede ser negativo."));

        var pref = await preferences.FindByUserIdAsync(userId, ct);
        if (pref is null)
        {
            pref = new UserPreference { UserId = userId };
            await preferences.AddAsync(pref, ct);
        }

        pref.PreferredCategory = resource.PreferredCategory?.Trim();
        pref.MaxBudget = resource.MaxBudget;
        pref.PreferredDistrict = resource.PreferredDistrict?.Trim();
        if (resource.NotificationsEnabled is not null)
            pref.NotificationsEnabled = resource.NotificationsEnabled.Value;
        pref.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.CompleteAsync(ct);
        return Ok(ToResource(pref));
    }

    private static UserPreferenceResource ToResource(UserPreference p) =>
        new(p.UserId, p.PreferredCategory, p.MaxBudget, p.PreferredDistrict, p.NotificationsEnabled);
}
