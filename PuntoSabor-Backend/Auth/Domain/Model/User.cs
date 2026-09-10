using PuntoSabor_Backend.Shared.Domain.Model;

namespace PuntoSabor_Backend.Auth.Domain.Model;

/// <summary>
/// Represents a registered user in the PuntoSabor platform.
/// </summary>
public class User : AuditableEntity
{
    /// <summary>Display name of the user.</summary>
    public string Name { get; set; } = null!;

    /// <summary>Unique email address used for authentication.</summary>
    public string Email { get; set; } = null!;

    /// <summary>Bcrypt hashed password for secure authentication.</summary>
    public string PasswordHash { get; set; } = null!;

    /// <summary>Role assigned to the user: Consumer or Owner.</summary>
    public UserRole Role { get; set; } = UserRole.Consumer;
}