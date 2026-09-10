using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using PuntoSabor_Backend.Auth.Domain.Model;
using PuntoSabor_Backend.Auth.Domain.Repositories;
using PuntoSabor_Backend.Presentation.Resources;
using PuntoSabor_Backend.Presentation.Transform;
using PuntoSabor_Backend.Shared.Domain.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace PuntoSabor_Backend.Presentation.Controllers;

/**
 * <summary>
 *     Controlador para buscar y crear usuarios.
 * </summary>
 */
[ApiController]
[Route("users")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Users Endpoints.")]
public class UsersController(
    IUserRepository users,
    IUnitOfWork unitOfWork) : ControllerBase
{
    /**
     * <summary>
     *     GET /users?email=demo@puntosabor.com
     *     Busca usuarios filtrando por email.
     * </summary>
     */
    [HttpGet]
    [SwaggerOperation("Search Users", "Search users by email filter.", OperationId = "SearchUsers")]
    [SwaggerResponse(200, "Usuarios encontrados y retornados.", typeof(IEnumerable<UserResource>))]
    [SwaggerResponse(400, "Solicitud inválida. Verifica los parámetros enviados.", typeof(IEnumerable<UserResource>))]
    [SwaggerResponse(404, "No se encontraron usuarios con los filtros enviados.", typeof(IEnumerable<UserResource>))]
    public async Task<IActionResult> Search([FromQuery] string? email, CancellationToken ct)
    {
        if (email is not null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new ErrorResource("El parámetro 'email' no puede estar vacío."));

            email = email.Trim();

            if (email.Length < 5)
                return BadRequest(new ErrorResource("El parámetro 'email' es demasiado corto."));

            if (email.Length > 120)
                return BadRequest(new ErrorResource("El parámetro 'email' excede el máximo permitido (120 caracteres)."));

            if (!email.Contains('@') || email.StartsWith('@') || email.EndsWith('@'))
                return BadRequest(new ErrorResource("El parámetro 'email' no tiene un formato válido."));
        }

        var result = await users.FindByEmailAsync(email, ct);

        var hasFilters = email is not null;
        if (hasFilters && !result.Any())
            return NotFound(new ErrorResource("No se encontraron usuarios con los filtros enviados."));

        var resources = result.Select(UserResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation("Get User By Id", "Returns a single user by ID.", OperationId = "GetUserById")]
    [SwaggerResponse(200, "Usuario encontrado.", typeof(UserResource))]
    [SwaggerResponse(404, "Usuario no encontrado.", typeof(ErrorResource))]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(id, ct);
        if (user is null)
            return NotFound(new ErrorResource("Usuario no encontrado."));

        return Ok(UserResourceFromEntityAssembler.ToResourceFromEntity(user));
    }

    /**
     * <summary>
     *     Crea un nuevo usuario.
     *     POST /users
     * </summary>
     */
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [SwaggerOperation("Create User", "Create a new user.", OperationId = "CreateUser")]
    [SwaggerResponse(201, "El usuario fue creado.", typeof(UserResource))]
    [SwaggerResponse(400, "El usuario no pudo ser creado.", typeof(UserResource))]
    public async Task<IActionResult> Create([FromBody] CreateUserResource resource, CancellationToken ct)
    {
        if (resource is null)
            return BadRequest(new ErrorResource("Body inválido."));

        if (string.IsNullOrWhiteSpace(resource.Name))
            return BadRequest(new ErrorResource("El campo 'name' es obligatorio."));

        if (string.IsNullOrWhiteSpace(resource.Email))
            return BadRequest(new ErrorResource("El campo 'email' es obligatorio."));

        if (string.IsNullOrWhiteSpace(resource.Password))
            return BadRequest(new ErrorResource("El campo 'password' es obligatorio."));

        if (resource.Password.Length < 6)
            return BadRequest(new ErrorResource("La contraseña debe tener al menos 6 caracteres."));

        var name = resource.Name.Trim();
        var email = resource.Email.Trim();

        if (name.Length < 2)
            return BadRequest(new ErrorResource("El campo 'name' debe tener al menos 2 caracteres."));

        if (name.Length > 80)
            return BadRequest(new ErrorResource("El campo 'name' excede el máximo permitido (80 caracteres)."));

        if (email.Length > 120)
            return BadRequest(new ErrorResource("El campo 'email' excede el máximo permitido (120 caracteres)."));

        if (!email.Contains('@') || email.StartsWith('@') || email.EndsWith('@'))
            return BadRequest(new ErrorResource("El campo 'email' no tiene un formato válido."));

        var existing = await users.FindByEmailAsync(email, ct);
        if (existing.Any())
            return BadRequest(new ErrorResource("Ya existe un usuario con ese email."));

        var entity = CreateUserEntityFromResourceAssembler.ToEntityFromResource(resource);

        await users.AddAsync(entity, ct);
        await unitOfWork.CompleteAsync(ct);

        var createdResource = UserResourceFromEntityAssembler.ToResourceFromEntity(entity);

        return CreatedAtAction(nameof(Search), new { email = entity.Email }, createdResource);
    }

    [HttpPatch("{id:int}/role")]
    [Consumes(MediaTypeNames.Application.Json)]
    [SwaggerOperation("Update User Role", "Changes the role of an existing user (consumer/owner).", OperationId = "UpdateUserRole")]
    [SwaggerResponse(200, "Rol actualizado.", typeof(UserResource))]
    [SwaggerResponse(400, "Rol inválido.", typeof(ErrorResource))]
    [SwaggerResponse(404, "Usuario no encontrado.", typeof(ErrorResource))]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateUserRoleResource resource, CancellationToken ct)
    {
        if (resource is null || string.IsNullOrWhiteSpace(resource.Role))
            return BadRequest(new ErrorResource("El campo 'role' es obligatorio."));

        var normalized = resource.Role.Trim().ToLowerInvariant();
        if (normalized != "consumer" && normalized != "owner")
            return BadRequest(new ErrorResource("El rol debe ser 'consumer' u 'owner'."));

        var user = await users.FindByIdAsync(id, ct);
        if (user is null)
            return NotFound(new ErrorResource("Usuario no encontrado."));

        user.Role = normalized == "owner" ? UserRole.Owner : UserRole.Consumer;
        user.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.CompleteAsync(ct);

        return Ok(UserResourceFromEntityAssembler.ToResourceFromEntity(user));
    }
}
