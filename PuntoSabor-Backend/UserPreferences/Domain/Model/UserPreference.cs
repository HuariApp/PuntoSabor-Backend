using PuntoSabor_Backend.Shared.Domain.Model;

namespace PuntoSabor_Backend.UserPreferences.Domain.Model;

/**
 * <summary>
 *     Preferencias del usuario (US17) y configuración de notificaciones (US11).
 * </summary>
 */
public class UserPreference : AuditableEntity
{
    public int UserId { get; set; }

    /// Categoría de cocina preferida (nombre).
    public string? PreferredCategory { get; set; }

    /// Presupuesto máximo por persona (S/).
    public decimal? MaxBudget { get; set; }

    /// Distrito preferido para recomendaciones.
    public string? PreferredDistrict { get; set; }

    /// Notificaciones activadas (US11).
    public bool NotificationsEnabled { get; set; } = true;
}
