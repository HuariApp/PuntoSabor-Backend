namespace PuntoSabor_Backend.Presentation.Resources;

/**
 * <summary>
 *     Representación de un huarique para respuestas de la API.
 * </summary>
 */

public record HuariqueResource(
    int Id,
    string Name,
    string Category,
    int CategoryId,
    decimal Price,
    double Rating,
    string District,
    bool Near,
    double? Latitude,
    double? Longitude,
    int? OwnerId,
    string? Address,
    string? Phone,
    string? Description,
    string? ImageUrl,
    string? OpenAt,
    string? CloseAt,
    bool DeliveryAvailable,
    bool TakeawayAvailable,
    bool DineInAvailable,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);