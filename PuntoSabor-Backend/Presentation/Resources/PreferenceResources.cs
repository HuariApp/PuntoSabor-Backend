namespace PuntoSabor_Backend.Presentation.Resources;

/// Respuesta con las preferencias del usuario (US17/US11).
public record UserPreferenceResource(
    int UserId,
    string? PreferredCategory,
    decimal? MaxBudget,
    string? PreferredDistrict,
    bool NotificationsEnabled
);

/// Cuerpo para guardar/actualizar preferencias.
public record UpdatePreferenceResource(
    string? PreferredCategory,
    decimal? MaxBudget,
    string? PreferredDistrict,
    bool? NotificationsEnabled
);
