namespace PuntoSabor_Backend.Presentation.Resources;

/// Respuesta de una notificación (US12).
public record NotificationResource(
    int Id,
    int UserId,
    string Title,
    string Body,
    bool IsRead,
    DateTime CreatedAt
);
