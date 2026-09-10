using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using PuntoSabor_Backend.Auth.Application.Services;
using PuntoSabor_Backend.Auth.Domain.Repositories;
using PuntoSabor_Backend.Presentation.Resources;
using PuntoSabor_Backend.Shared.Domain.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace PuntoSabor_Backend.Presentation.Controllers;

[ApiController]
[Route("auth")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Authentication Endpoints.")]
public class AuthController(
    IUserRepository users,
    ITokenService tokenService,
    IUnitOfWork unitOfWork) : ControllerBase
{
 /**
   * <summary>
   *     Autentica al usuario con email y contraseña; devuelve un JWT.
   *     POST /auth/login
   * </summary>
   */
    [HttpPost("login")]
    [Consumes(MediaTypeNames.Application.Json)]
    [SwaggerOperation("Login", "Authenticate with email and password, returns a JWT token.", OperationId = "Login")]
    [SwaggerResponse(200, "Autenticación exitosa.", typeof(AuthResponseResource))]
    [SwaggerResponse(400, "Datos inválidos.", typeof(ErrorResource))]
    [SwaggerResponse(401, "Credenciales incorrectas.", typeof(ErrorResource))]
    public async Task<IActionResult> Login([FromBody] LoginResource resource, CancellationToken ct)
    {
        if (resource is null)
            return BadRequest(new ErrorResource("Body inválido."));

        if (string.IsNullOrWhiteSpace(resource.Email) || string.IsNullOrWhiteSpace(resource.Password))
            return BadRequest(new ErrorResource("Email y contraseña son obligatorios."));

        var matches = await users.FindByEmailAsync(resource.Email.Trim(), ct);
        var user = matches.FirstOrDefault();

        if (user is null || !BCrypt.Net.BCrypt.Verify(resource.Password, user.PasswordHash))
            return Unauthorized(new ErrorResource("Credenciales inválidas."));

        var token = tokenService.GenerateToken(user);
        return Ok(new AuthResponseResource(user.Id, user.Name, user.Email,
            user.Role.ToString().ToLowerInvariant(), token));
    }

 /**
   * <summary>
   *     Obtiene el perfil público de un usuario por su identificador.
   *     GET /auth/users/:id
   * </summary>
   */
    [HttpGet("users/{id:int}")]
    [SwaggerOperation("Get Profile", "Returns the profile of a user by ID.", OperationId = "GetProfile")]
    [SwaggerResponse(200, "Perfil encontrado.", typeof(UserResource))]
    [SwaggerResponse(404, "Usuario no encontrado.", typeof(ErrorResource))]
    public async Task<IActionResult> GetProfile(int id, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(id, ct);
        if (user is null)
            return NotFound(new ErrorResource("Usuario no encontrado."));

        return Ok(new UserResource(user.Id, user.Name, user.Email,
            user.Role.ToString().ToLowerInvariant(), user.CreatedAt, user.UpdatedAt));
    }

/**
   * <summary>
   *     Actualiza el nombre visible del perfil de un usuario.
   *     PATCH /auth/users/:id
   * </summary>
   */
    [HttpPatch("users/{id:int}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [SwaggerOperation("Update Profile", "Updates the display name of a user.", OperationId = "UpdateProfile")]
    [SwaggerResponse(200, "Perfil actualizado.", typeof(UserResource))]
    [SwaggerResponse(400, "Datos inválidos.", typeof(ErrorResource))]
    [SwaggerResponse(404, "Usuario no encontrado.", typeof(ErrorResource))]
    public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateProfileResource resource, CancellationToken ct)
    {
        if (resource is null || string.IsNullOrWhiteSpace(resource.Name))
            return BadRequest(new ErrorResource("El campo 'name' es obligatorio."));

        var name = resource.Name.Trim();

        if (name.Length < 2)
            return BadRequest(new ErrorResource("El campo 'name' debe tener al menos 2 caracteres."));

        if (name.Length > 80)
            return BadRequest(new ErrorResource("El campo 'name' excede el máximo permitido (80 caracteres)."));

        var user = await users.FindTrackedByIdAsync(id, ct);
        if (user is null)
            return NotFound(new ErrorResource("Usuario no encontrado."));

        user.Name = name;
        user.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.CompleteAsync(ct);

        return Ok(new UserResource(user.Id, user.Name, user.Email,
            user.Role.ToString().ToLowerInvariant(), user.CreatedAt, user.UpdatedAt));
    }

/**
   * <summary>
   *     Elimina permanentemente la cuenta de un usuario.
   *     DELETE /auth/users/:id
   * </summary>
   */
    [HttpDelete("users/{id:int}")]
    [SwaggerOperation("Delete Account", "Permanently deletes a user account.", OperationId = "DeleteAccount")]
    [SwaggerResponse(200, "Cuenta eliminada.", typeof(MessageResource))]
    [SwaggerResponse(404, "Usuario no encontrado.", typeof(ErrorResource))]
    public async Task<IActionResult> DeleteAccount(int id, CancellationToken ct)
    {
        var user = await users.FindTrackedByIdAsync(id, ct);
        if (user is null)
            return NotFound(new ErrorResource("Usuario no encontrado."));

        users.Remove(user);
        await unitOfWork.CompleteAsync(ct);

        return Ok(new MessageResource("Cuenta eliminada correctamente."));
    }

/**
   * <summary>
   *     Inicia el flujo de recuperación de contraseña (US16).
   *     POST /auth/forgot-password
   * </summary>
   */
    [HttpPost("forgot-password")]
    [Consumes(MediaTypeNames.Application.Json)]
    [SwaggerOperation("Forgot Password", "Starts the password recovery flow for a registered email.", OperationId = "ForgotPassword")]
    [SwaggerResponse(200, "Si el correo existe, se envían instrucciones.", typeof(MessageResource))]
    [SwaggerResponse(400, "Datos inválidos.", typeof(ErrorResource))]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordResource resource, CancellationToken ct)
    {
        if (resource is null || string.IsNullOrWhiteSpace(resource.Email))
            return BadRequest(new ErrorResource("El campo 'email' es obligatorio."));

        var matches = await users.FindByEmailAsync(resource.Email.Trim(), ct);
        var exists = matches.Any();

        // Respuesta uniforme para no revelar si el correo está registrado (US16).
        return Ok(new MessageResource(exists
            ? "Si el correo está registrado, enviaremos instrucciones para restablecer la contraseña."
            : "Si el correo está registrado, enviaremos instrucciones para restablecer la contraseña."));
    }

 /**
   * <summary>
   *     Establece una nueva contraseña para un correo registrado.
   *     POST /auth/reset-password
   * </summary>
   */
    [HttpPost("reset-password")]
    [Consumes(MediaTypeNames.Application.Json)]
    [SwaggerOperation("Reset Password", "Sets a new password for a registered email.", OperationId = "ResetPassword")]
    [SwaggerResponse(200, "Contraseña actualizada.", typeof(MessageResource))]
    [SwaggerResponse(400, "Datos inválidos.", typeof(ErrorResource))]
    [SwaggerResponse(404, "Correo no registrado.", typeof(ErrorResource))]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordResource resource, CancellationToken ct)
    {
        if (resource is null || string.IsNullOrWhiteSpace(resource.Email))
            return BadRequest(new ErrorResource("El campo 'email' es obligatorio."));
        if (string.IsNullOrWhiteSpace(resource.NewPassword) || resource.NewPassword.Length < 6)
            return BadRequest(new ErrorResource("La nueva contraseña debe tener al menos 6 caracteres."));

        var user = await users.FindTrackedByEmailAsync(resource.Email.Trim(), ct);
        if (user is null)
            return NotFound(new ErrorResource("No existe una cuenta con ese correo."));

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resource.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.CompleteAsync(ct);

        return Ok(new MessageResource("Contraseña actualizada correctamente."));
    }
}
