using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using PuntoSabor_Backend.Discovery.Domain.Repositories;
using PuntoSabor_Backend.Favorites.Domain.Repositories;
using PuntoSabor_Backend.Presentation.Resources;
using PuntoSabor_Backend.Presentation.Transform;
using PuntoSabor_Backend.Shared.Domain.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace PuntoSabor_Backend.Presentation.Controllers;

[ApiController]
[Route("huariques")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Huariques Endpoints.")]
public class HuariquesController(
    IHuariqueRepository huariques,
    IFavoriteRepository favorites,
    IUnitOfWork unitOfWork) : ControllerBase
{
    /**
     * <summary>
     *     Sugerencias de huariques para un usuario (US18). Se basan en las
     *     categorías de sus favoritos; si no tiene favoritos, devuelve los
     *     mejor calificados.
     * </summary>
     */
    [HttpGet("suggestions")]
    [SwaggerOperation("Suggest Huariques", "Suggests huariques for a user based on favorites history.", OperationId = "SuggestHuariques")]
    [SwaggerResponse(200, "Sugerencias.", typeof(IEnumerable<HuariqueResource>))]
    [SwaggerResponse(400, "Parámetro inválido.", typeof(ErrorResource))]
    public async Task<IActionResult> Suggestions([FromQuery] int userId, CancellationToken ct)
    {
        if (userId <= 0)
            return BadRequest(new ErrorResource("El parámetro 'userId' debe ser mayor a 0."));

        var all = (await huariques.SearchAsync(null, null, null, ct)).ToList();
        var favoriteIds = (await favorites.FindByUserIdAsync(userId, ct))
            .Select(f => f.HuariqueId).ToHashSet();

        var favoriteCategories = all
            .Where(h => favoriteIds.Contains(h.Id))
            .Select(h => h.Category)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .ToHashSet();

        IEnumerable<Discovery.Domain.Model.Huarique> suggestions = favoriteCategories.Count > 0
            // Mismo tipo de comida que ya marcó como favorito, excluyendo los favoritos.
            ? all.Where(h => favoriteCategories.Contains(h.Category) && !favoriteIds.Contains(h.Id))
                 .OrderByDescending(h => h.Rating)
            // Sin historial: populares de la zona (mejor calificados).
            : all.OrderByDescending(h => h.Rating);

        var resources = suggestions.Take(10).Select(HuariqueResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    /**
     * <summary>
     *     Busca huariques por texto y/o cercanos.
     * </summary>
     */
    
    [HttpGet]
    [SwaggerOperation("Search Huariques", "Search huariques by text and/or near filter.", OperationId = "SearchHuariques")]
    [SwaggerResponse(200, "Huariques encontrados y retornados.", typeof(IEnumerable<HuariqueResource>))]
    [SwaggerResponse(400, "Solicitud inválida. Verifica los parámetros enviados.", typeof(IEnumerable<HuariqueResource>))]
    [SwaggerResponse(404, "No se encontraron huariques con los filtros enviados.", typeof(IEnumerable<HuariqueResource>))]
    public async Task<IActionResult> Search(
        [FromQuery] string? q,
        [FromQuery] bool? near,
        [FromQuery] int? ownerId,
        CancellationToken ct)
    {
        if (q is not null)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new ErrorResource("El parámetro 'q' no puede estar vacío."));

            q = q.Trim();

            if (q.Length < 2)
                return BadRequest(new ErrorResource("El parámetro 'q' debe tener al menos 2 caracteres."));

            if (q.Length > 80)
                return BadRequest(new ErrorResource("El parámetro 'q' excede el máximo permitido (80 caracteres)."));
        }

        var result = await huariques.SearchAsync(q, near, ownerId, ct);

        var hasFilters = q is not null || near is not null || ownerId is not null;
        if (hasFilters && !result.Any())
            return NotFound(new ErrorResource("No se encontraron huariques con los filtros enviados."));

        var resources = result.Select(HuariqueResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    /**
     * <summary>
     *     Devuelve el detalle de un huarique por id.
     *     GET /huariques/:id
     * </summary>
     */
    
    [HttpGet("{id:int}")]
    [SwaggerOperation("Get Huarique by Id", "Get a huarique by its unique identifier.", OperationId = "GetHuariqueById")]
    [SwaggerResponse(200, "El huarique fue encontrado y retornado.", typeof(HuariqueResource))]
    [SwaggerResponse(404, "El huarique no fue encontrado.", typeof(HuariqueResource))]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var entity = await huariques.FindByIdAsync(id, ct);
        if (entity is null) return NotFound(new ErrorResource("El huarique no fue encontrado."));

        var resource = HuariqueResourceFromEntityAssembler.ToResourceFromEntity(entity);
        return Ok(resource);
    }

    /**
    * <summary>
    *     Crea un nuevo huarique.
    * </summary>
    */
    
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [SwaggerOperation("Create Huarique", "Create a new huarique.", OperationId = "CreateHuarique")]
    [SwaggerResponse(201, "El huarique fue creado.", typeof(HuariqueResource))]
    [SwaggerResponse(400, "El huarique no pudo ser creado.", typeof(HuariqueResource))]
    public async Task<IActionResult> Create([FromBody] CreateHuariqueResource resource, CancellationToken ct)
    {
        if (resource is null)
            return BadRequest(new ErrorResource("Body inválido."));

        if (string.IsNullOrWhiteSpace(resource.Name))
            return BadRequest(new ErrorResource("El campo 'name' es obligatorio."));

        if (string.IsNullOrWhiteSpace(resource.District))
            return BadRequest(new ErrorResource("El campo 'district' es obligatorio."));

        if (resource.CategoryId <= 0)
            return BadRequest(new ErrorResource("El campo 'categoryId' debe ser mayor a 0."));

        if (resource.Price < 0)
            return BadRequest(new ErrorResource("El campo 'price' no puede ser negativo."));

        var entity = CreateHuariqueEntityFromResourceAssembler.ToEntityFromResource(resource);

        await huariques.AddAsync(entity, ct);
        await unitOfWork.CompleteAsync(ct);

        var createdResource = HuariqueResourceFromEntityAssembler.ToResourceFromEntity(entity);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, createdResource);
    }

    /**
     * <summary>
     *     Actualiza parcialmente un huarique.
     *     PATCH /huariques/:id
     * </summary>
     */
    
    [HttpPatch("{id:int}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [SwaggerOperation("Patch Huarique", "Update specific fields of an existing huarique.", OperationId = "PatchHuarique")]
    [SwaggerResponse(200, "El huarique fue actualizado parcialmente.", typeof(HuariqueResource))]
    [SwaggerResponse(400, "Solicitud inválida. Verifica el contenido enviado.", typeof(HuariqueResource))]
    [SwaggerResponse(404, "El huarique no fue encontrado.", typeof(HuariqueResource))]
    public async Task<IActionResult> Patch(int id, [FromBody] Dictionary<string, object> patch, CancellationToken ct)
    {
        if (patch is null || patch.Count == 0)
            return BadRequest(new ErrorResource("No se enviaron campos para actualizar."));

        var existing = await huariques.FindByIdAsync(id, ct);
        if (existing is null)
            return NotFound(new ErrorResource("El huarique no fue encontrado."));

        await huariques.PatchAsync(existing, patch, ct);
        await unitOfWork.CompleteAsync(ct);

        var updatedResource = HuariqueResourceFromEntityAssembler.ToResourceFromEntity(existing);
        return Ok(updatedResource);
    }

    [HttpDelete("{id:int}")]
    [SwaggerOperation("Delete Huarique", "Deletes a huarique by its owner.", OperationId = "DeleteHuarique")]
    [SwaggerResponse(204, "El huarique fue eliminado.")]
    [SwaggerResponse(404, "El huarique no fue encontrado.", typeof(ErrorResource))]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var existing = await huariques.FindByIdAsync(id, ct);
        if (existing is null)
            return NotFound(new ErrorResource("El huarique no fue encontrado."));

        await huariques.DeleteAsync(existing, ct);
        await unitOfWork.CompleteAsync(ct);
        return NoContent();
    }
    
    /**
     * <summary>
     *     Sube o reemplaza la imagen de un huarique (la guarda en la base de datos).
     *     POST /huariques/:id/image  (multipart/form-data, campo "file")
     * </summary>
     */
    [HttpPost("{id:int}/image")]
    [Consumes("multipart/form-data")]
    [SwaggerOperation("Upload Huarique Image", "Uploads or replaces the huarique image stored in the database.", OperationId = "UploadHuariqueImage")]
    [SwaggerResponse(200, "Imagen guardada.", typeof(HuariqueResource))]
    [SwaggerResponse(400, "Archivo inválido.", typeof(ErrorResource))]
    [SwaggerResponse(404, "El huarique no fue encontrado.", typeof(ErrorResource))]
    public async Task<IActionResult> UploadImage(int id, [FromForm] IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new ErrorResource("Debes enviar un archivo en el campo 'file'."));

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new ErrorResource("La imagen no puede superar los 5 MB."));

        var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowed.Contains(file.ContentType))
            return BadRequest(new ErrorResource("Formato no permitido. Usa JPEG, PNG o WEBP."));

        var entity = await huariques.FindByIdAsync(id, ct);
        if (entity is null)
            return NotFound(new ErrorResource("El huarique no fue encontrado."));

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);

        entity.ImageData = ms.ToArray();
        entity.ImageContentType = file.ContentType;
        entity.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.CompleteAsync(ct);

        return Ok(HuariqueResourceFromEntityAssembler.ToResourceFromEntity(entity));
    }

    /**
     * <summary>
     *     Devuelve la imagen del huarique guardada en la base de datos.
     *     GET /huariques/:id/image
     * </summary>
     */
    [HttpGet("{id:int}/image")]
    [SwaggerOperation("Get Huarique Image", "Returns the huarique image stored in the database.", OperationId = "GetHuariqueImage")]
    [SwaggerResponse(200, "Imagen encontrada.")]
    [SwaggerResponse(404, "El huarique no tiene imagen.", typeof(ErrorResource))]
    public async Task<IActionResult> GetImage(int id, CancellationToken ct)
    {
        var image = await huariques.GetImageAsync(id, ct);
        if (image is null)
            return NotFound(new ErrorResource("El huarique no tiene imagen."));

        return File(image.Value.Data, image.Value.ContentType);
    }

    /**
     * <summary>
     *     Elimina la imagen del huarique.
     *     DELETE /huariques/:id/image
     * </summary>
     */
    [HttpDelete("{id:int}/image")]
    [SwaggerOperation("Delete Huarique Image", "Removes the huarique image from the database.", OperationId = "DeleteHuariqueImage")]
    [SwaggerResponse(204, "Imagen eliminada.")]
    [SwaggerResponse(404, "El huarique no fue encontrado.", typeof(ErrorResource))]
    public async Task<IActionResult> DeleteImage(int id, CancellationToken ct)
    {
        var entity = await huariques.FindByIdAsync(id, ct);
        if (entity is null)
            return NotFound(new ErrorResource("El huarique no fue encontrado."));

        entity.ImageData = null;
        entity.ImageContentType = null;
        entity.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.CompleteAsync(ct);
        return NoContent();
    }
}
