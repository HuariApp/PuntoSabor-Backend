using PuntoSabor_Backend.Shared.Domain.Model;

namespace PuntoSabor_Backend.Reviews.Domain.Model;

/// <summary>
/// Represents a review submitted by a user for a huarique in the PuntoSabor platform.
/// </summary>
public class Review : AuditableEntity
{
    /// <summary>Identifier of the huarique being reviewed.</summary>
    public int HuariqueId { get; set; }

    /// <summary>Identifier of the user who submitted the review.</summary>
    public int UserId { get; set; }

    /// <summary>Numeric rating given by the user, typically between 1 and 5.</summary>
    public int Rating { get; set; }

    /// <summary>Written comment describing the user's experience.</summary>
    public string Comment { get; set; } = null!;

    /// <summary>Timestamp indicating when the review was created.</summary>
    public DateTime CreatedAtReview { get; set; }
}