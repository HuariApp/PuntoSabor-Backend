using PuntoSabor_Backend.Shared.Domain.Model;

namespace PuntoSabor_Backend.Notifications.Domain.Model;

/// <summary>
/// Represents a notification sent to a user in the PuntoSabor platform.
/// Notifications are triggered by events such as new reviews on owned huariques.
/// </summary>
public class Notification : AuditableEntity
{
    /// <summary>Identifier of the user who receives this notification.</summary>
    public int UserId { get; set; }

    /// <summary>Short title summarizing the notification content.</summary>
    public string Title { get; set; } = null!;

    /// <summary>Detailed body message of the notification.</summary>
    public string Body { get; set; } = null!;

    /// <summary>Indicates whether the user has already read this notification.</summary>
    public bool IsRead { get; set; }
}