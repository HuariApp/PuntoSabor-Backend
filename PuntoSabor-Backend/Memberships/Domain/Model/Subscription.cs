using PuntoSabor_Backend.Shared.Domain.Model;

namespace PuntoSabor_Backend.Memberships.Domain.Model;

/// <summary>
/// Represents a membership subscription of a user to a plan in PuntoSabor.
/// </summary>
public class Subscription : AuditableEntity
{
    /// <summary>Identifier of the user who owns this subscription.</summary>
    public int UserId { get; set; }

    /// <summary>Identifier of the plan associated with this subscription.</summary>
    public string PlanId { get; set; } = null!;

    /// <summary>Navigation property to the associated plan.</summary>
    public Plan? Plan { get; set; }

    /// <summary>Date when the subscription started.</summary>
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    /// <summary>Date when the subscription ends. Null means no expiration.</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>Current status of the subscription: active | cancelled | expired.</summary>
    public string Status { get; set; } = "active";

    /// <summary>Indicates whether the subscription is currently active and not expired.</summary>
    public bool IsActive => Status == "active"
        && (EndDate == null || EndDate >= DateTime.UtcNow);
}