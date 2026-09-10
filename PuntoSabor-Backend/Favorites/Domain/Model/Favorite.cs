using PuntoSabor_Backend.Shared.Domain.Model;

namespace PuntoSabor_Backend.Favorites.Domain.Model;

/// <summary>
/// Represents a huarique marked as favorite by a user in the PuntoSabor platform.
/// </summary>
public class Favorite : AuditableEntity
{
    /// <summary>Identifier of the user who marked the huarique as favorite.</summary>
    public int UserId { get; set; }

    /// <summary>Identifier of the huarique saved as favorite.</summary>
    public int HuariqueId { get; set; }
}