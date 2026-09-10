namespace PuntoSabor_Backend.Presentation.Resources;

public record PromoResource(
    int Id,
    string Title,
    string Note,
    string Type,
    int Discount,
    string? Code,
    DateTime? StartDate,
    DateTime? EndDate,
    int? MaxUses,
    int CurrentUses,
    int? HuariqueId,
    string? ImageUrl,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
