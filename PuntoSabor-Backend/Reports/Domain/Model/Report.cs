using PuntoSabor_Backend.Shared.Domain.Model;

namespace PuntoSabor_Backend.Reports.Domain.Model;

/// <summary>
/// Represents a report submitted by a user about incorrect information on a huarique.
/// </summary>
public class Report : AuditableEntity
{
    /// <summary>Identifier of the huarique being reported.</summary>
    public int HuariqueId { get; set; }

    /// <summary>Identifier of the user who submitted the report.</summary>
    public int UserId { get; set; }

    /// <summary>Description of the incorrect information found in the huarique.</summary>
    public string Reason { get; set; } = null!;

    /// <summary>Current status of the report: pending | reviewed.</summary>
    public string Status { get; set; } = "pending";
}