using PuntoSabor_Backend.Shared.Domain.Model;

namespace PuntoSabor_Backend.Promotions.Domain.Model;

/// <summary>
/// Represents a promotional offer associated with a huarique in the PuntoSabor platform.
/// </summary>
public class Promo : AuditableEntity
{
    /// <summary>Title displayed to users for this promotion.</summary>
    public string Title { get; set; } = null!;

    /// <summary>Additional note or condition for the promotion.</summary>
    public string Note { get; set; } = null!;

    /// <summary>Promotion type: 2x1 | descuento | menu | happy-hour | otro.</summary>
    public string Type { get; set; } = "otro";

    /// <summary>Discount percentage. Zero if not applicable.</summary>
    public int Discount { get; set; } = 0;

    /// <summary>Optional redemption code for the promotion.</summary>
    public string? Code { get; set; }

    /// <summary>Date when the promotion becomes active.</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>Date when the promotion expires.</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>Maximum number of uses allowed. Null means unlimited.</summary>
    public int? MaxUses { get; set; }

    /// <summary>Current number of times this promotion has been used.</summary>
    public int CurrentUses { get; set; } = 0;

    /// <summary>Identifier of the huarique this promotion belongs to.</summary>
    public int? HuariqueId { get; set; }

    /// <summary>Optional image URL for the promotion banner.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Indicates whether the promotion is currently active.</summary>
    public bool IsActive =>
        (StartDate == null || StartDate <= DateTime.UtcNow) &&
        (EndDate   == null || EndDate   >= DateTime.UtcNow) &&
        (MaxUses   == null || CurrentUses < MaxUses);
}