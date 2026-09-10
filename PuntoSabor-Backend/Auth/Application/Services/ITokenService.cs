using PuntoSabor_Backend.Auth.Domain.Model;

namespace PuntoSabor_Backend.Auth.Application.Services;

/// <summary>
/// Defines the contract for generating JWT tokens used by authenticated users.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a signed JWT token using the user's identity and role.
    /// </summary>
    /// <param name="user">Authenticated user information.</param>
    /// <returns>A JWT token string.</returns>
    string GenerateToken(User user);
}