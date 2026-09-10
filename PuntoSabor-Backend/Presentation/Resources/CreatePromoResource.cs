namespace PuntoSabor_Backend.Presentation.Resources;

public record CreatePromoResource(
    string Title,
    string Note,
    int? HuariqueId = null,
    string Type = "otro",
    int Discount = 0,
    string? Code = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int? MaxUses = null,
    string? ImageUrl = null);
