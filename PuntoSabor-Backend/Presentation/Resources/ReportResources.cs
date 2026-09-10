namespace PuntoSabor_Backend.Presentation.Resources;

/// Respuesta de un reporte de información incorrecta (US21).
public record ReportResource(
    int Id,
    int HuariqueId,
    int UserId,
    string Reason,
    string Status,
    DateTime CreatedAt
);

/// Cuerpo para crear un reporte.
public record CreateReportResource(
    int HuariqueId,
    int UserId,
    string Reason
);
