using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using PuntoSabor_Backend.Presentation.Resources;
using PuntoSabor_Backend.Presentation.Transform;
using PuntoSabor_Backend.Promotions.Domain.Repositories;
using PuntoSabor_Backend.Shared.Domain.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace PuntoSabor_Backend.Presentation.Controllers;

[ApiController]
[Route("promos")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Promos Endpoints.")]
public class PromosController(
    IPromoRepository promos,
    IUnitOfWork unitOfWork) : ControllerBase
{
/**
     * <summary>
     *     Lista promociones con filtros opcionales por huarique, dueño o estado.
     *     GET /promos
     * </summary>
     */
    [HttpGet]
    [SwaggerOperation("List Promos", "Returns promos with optional filters.", OperationId = "ListPromos")]
    [SwaggerResponse(200, "Lista de promos.", typeof(IEnumerable<PromoResource>))]
    public async Task<IActionResult> List(
        [FromQuery] int? huariqueId,
        [FromQuery] int? ownerId,
        [FromQuery] bool? active,
        CancellationToken ct)
    {
        IEnumerable<PuntoSabor_Backend.Promotions.Domain.Model.Promo> result;

        if (ownerId.HasValue)
            result = await promos.FindByOwnerIdAsync(ownerId.Value, ct);
        else if (huariqueId.HasValue)
            result = await promos.FindByHuariqueIdAsync(huariqueId.Value, ct);
        else
            result = await promos.ListAsync(ct);

        if (active.HasValue)
            result = active.Value
                ? result.Where(p => p.IsActive)
                : result.Where(p => !p.IsActive);

        return Ok(result.Select(PromoResourceFromEntityAssembler.ToResourceFromEntity));
    }

 /**
     * <summary>
     *     Devuelve el detalle de una promo por id.
     *     GET /promos/:id
     * </summary>
     */
    [HttpGet("{id:int}")]
    [SwaggerOperation("Get Promo", "Returns a single promo by ID.", OperationId = "GetPromo")]
    [SwaggerResponse(200, "Promo encontrada.", typeof(PromoResource))]
    [SwaggerResponse(404, "Promo no encontrada.", typeof(ErrorResource))]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var promo = await promos.FindByIdAsync(id, ct);
        if (promo is null) return NotFound(new ErrorResource("Promo no encontrada."));
        return Ok(PromoResourceFromEntityAssembler.ToResourceFromEntity(promo));
    }

/**
     * <summary>
     *     Crea una nueva promoción asociada a un huarique.
     *     POST /promos
     * </summary>
     */
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [SwaggerOperation("Create Promo", "Creates a new promo.", OperationId = "CreatePromo")]
    [SwaggerResponse(201, "Promo creada.", typeof(PromoResource))]
    [SwaggerResponse(400, "Datos inválidos.", typeof(ErrorResource))]
    public async Task<IActionResult> Create([FromBody] CreatePromoResource resource, CancellationToken ct)
    {
        if (resource is null)
            return BadRequest(new ErrorResource("Body inválido."));

        if (string.IsNullOrWhiteSpace(resource.Title))
            return BadRequest(new ErrorResource("El campo 'title' es obligatorio."));

        if (string.IsNullOrWhiteSpace(resource.Note))
            return BadRequest(new ErrorResource("El campo 'note' es obligatorio."));

        if (resource.Discount < 0 || resource.Discount > 100)
            return BadRequest(new ErrorResource("El campo 'discount' debe estar entre 0 y 100."));

        if (resource.StartDate.HasValue && resource.EndDate.HasValue
            && resource.EndDate < resource.StartDate)
            return BadRequest(new ErrorResource("'endDate' no puede ser anterior a 'startDate'."));

        var entity = CreatePromoEntityFromResourceAssembler.ToEntityFromResource(resource);
        await promos.AddAsync(entity, ct);
        await unitOfWork.CompleteAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = entity.Id },
            PromoResourceFromEntityAssembler.ToResourceFromEntity(entity));
    }

/**
     * <summary>
     *     Actualiza parcialmente los campos de una promo existente.
     *     PATCH /promos/:id
     * </summary>
     */
    [HttpPatch("{id:int}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [SwaggerOperation("Patch Promo", "Partially updates a promo.", OperationId = "PatchPromo")]
    [SwaggerResponse(200, "Promo actualizada.", typeof(PromoResource))]
    [SwaggerResponse(404, "Promo no encontrada.", typeof(ErrorResource))]
    public async Task<IActionResult> Patch(int id, [FromBody] Dictionary<string, object> patch, CancellationToken ct)
    {
        if (patch is null || patch.Count == 0)
            return BadRequest(new ErrorResource("No se enviaron campos para actualizar."));

        var promo = await promos.FindByIdAsync(id, ct);
        if (promo is null) return NotFound(new ErrorResource("Promo no encontrada."));

        foreach (var (key, value) in patch)
        {
            switch (key.ToLowerInvariant())
            {
                case "title":       promo.Title = value?.ToString() ?? promo.Title; break;
                case "note":        promo.Note = value?.ToString() ?? promo.Note; break;
                case "type":        promo.Type = value?.ToString()?.ToLowerInvariant() ?? promo.Type; break;
                case "discount":    promo.Discount = Convert.ToInt32(value); break;
                case "code":        promo.Code = value?.ToString(); break;
                case "startdate":   promo.StartDate = value is null ? null : Convert.ToDateTime(value); break;
                case "enddate":     promo.EndDate = value is null ? null : Convert.ToDateTime(value); break;
                case "maxuses":     promo.MaxUses = value is null ? null : Convert.ToInt32(value); break;
                case "huariqueid":  promo.HuariqueId = value is null ? null : Convert.ToInt32(value); break;
                case "imageurl":    promo.ImageUrl = value?.ToString(); break;
            }
        }

        promo.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.CompleteAsync(ct);
        return Ok(PromoResourceFromEntityAssembler.ToResourceFromEntity(promo));
    }

 /**
     * <summary>
     *     Elimina una promoción del catálogo.
     *     DELETE /promos/:id
     * </summary>
     */
    [HttpDelete("{id:int}")]
    [SwaggerOperation("Delete Promo", "Deletes a promo.", OperationId = "DeletePromo")]
    [SwaggerResponse(204, "Promo eliminada.")]
    [SwaggerResponse(404, "Promo no encontrada.", typeof(ErrorResource))]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var promo = await promos.FindByIdAsync(id, ct);
        if (promo is null) return NotFound(new ErrorResource("Promo no encontrada."));

        promos.Remove(promo);
        await unitOfWork.CompleteAsync(ct);
        return NoContent();
    }

/**
     * <summary>
     *     Registra un uso de la promo e incrementa su contador.
     *     POST /promos/:id/use
     * </summary>
     */
    [HttpPost("{id:int}/use")]
    [SwaggerOperation("Use Promo", "Increments the usage counter of a promo.", OperationId = "UsePromo")]
    [SwaggerResponse(200, "Uso registrado.", typeof(PromoResource))]
    [SwaggerResponse(400, "Promo sin usos disponibles.", typeof(ErrorResource))]
    [SwaggerResponse(404, "Promo no encontrada.", typeof(ErrorResource))]
    public async Task<IActionResult> Use(int id, CancellationToken ct)
    {
        var promo = await promos.FindByIdAsync(id, ct);
        if (promo is null) return NotFound(new ErrorResource("Promo no encontrada."));

        if (!promo.IsActive)
            return BadRequest(new ErrorResource("Esta promo ya no está activa o ha alcanzado el límite de usos."));

        promo.CurrentUses++;
        promo.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.CompleteAsync(ct);
        return Ok(PromoResourceFromEntityAssembler.ToResourceFromEntity(promo));
    }
}
