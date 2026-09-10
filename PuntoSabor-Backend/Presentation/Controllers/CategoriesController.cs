using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using PuntoSabor_Backend.Discovery.Domain.Model;
using PuntoSabor_Backend.Discovery.Domain.Repositories;
using PuntoSabor_Backend.Presentation.Resources;
using PuntoSabor_Backend.Presentation.Transform;
using PuntoSabor_Backend.Shared.Domain.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace PuntoSabor_Backend.Presentation.Controllers;

[ApiController]
[Route("categories")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Categories Endpoints.")]
public class CategoriesController(
    ICategoryRepository categories,
    IUnitOfWork unitOfWork) : ControllerBase
{
    /**
     * <summary>
     *     Devuelve todas las categorías de huariques.
     *     GET /categories
     * </summary>
     */
    [HttpGet]
    [SwaggerOperation("Get Categories", "Returns all huarique categories.", OperationId = "GetCategories")]
    [SwaggerResponse(200, "Categorías encontradas y retornadas.", typeof(IEnumerable<Category>))]
    public async Task<ActionResult<IEnumerable<Category>>> GetAll(CancellationToken ct)
    {
        var result = await categories.ListAsync(ct);
        return Ok(result);
    }

    /**
     * <summary>
     *     Crea una nueva categoría. Útil cuando un huarique ofrece un tipo de
     *     comida que no estaba contemplado en las categorías iniciales.
     *     POST /categories
     * </summary>
     */
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [SwaggerOperation("Create Category", "Create a new huarique category.", OperationId = "CreateCategory")]
    [SwaggerResponse(201, "La categoría fue creada.", typeof(Category))]
    [SwaggerResponse(400, "La categoría no pudo ser creada.", typeof(ErrorResource))]
    [SwaggerResponse(409, "Ya existe una categoría con ese nombre.", typeof(ErrorResource))]
    public async Task<IActionResult> Create([FromBody] CreateCategoryResource resource, CancellationToken ct)
    {
        if (resource is null || string.IsNullOrWhiteSpace(resource.Name))
            return BadRequest(new ErrorResource("El campo 'name' es obligatorio."));

        var name = resource.Name.Trim();

        if (name.Length < 2)
            return BadRequest(new ErrorResource("El campo 'name' debe tener al menos 2 caracteres."));

        if (name.Length > 50)
            return BadRequest(new ErrorResource("El campo 'name' excede el máximo permitido (50 caracteres)."));

        if (await categories.ExistsByNameAsync(name, ct))
            return Conflict(new ErrorResource($"Ya existe una categoría con el nombre '{name}'."));

        var entity = CreateCategoryEntityFromResourceAssembler.ToEntityFromResource(resource);

        await categories.AddAsync(entity, ct);
        await unitOfWork.CompleteAsync(ct);

        return Created($"/categories/{entity.Id}", entity);
    }
}
