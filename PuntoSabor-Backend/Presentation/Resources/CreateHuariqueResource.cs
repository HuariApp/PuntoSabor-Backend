namespace PuntoSabor_Backend.Presentation.Resources;

/**
 * <summary>
 *     Datos necesarios para crear un huarique.
 * </summary>
 */

public record CreateHuariqueResource(
    string Name,
    string Category,
    int CategoryId,
    decimal Price,
    string District,
    double? Latitude = null,
    double? Longitude = null,
    int? OwnerId = null,
    string? Address = null,
    string? Phone = null,
    string? Description = null,
    string? ImageUrl = null,
    string? OpenAt = null,
    string? CloseAt = null,
    bool DeliveryAvailable = false,
    bool TakeawayAvailable = false,
    bool DineInAvailable = true
);