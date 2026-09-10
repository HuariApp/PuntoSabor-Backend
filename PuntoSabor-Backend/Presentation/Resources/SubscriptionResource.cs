namespace PuntoSabor_Backend.Presentation.Resources;

public record SubscriptionResource(
    int Id,
    int UserId,
    string PlanId,
    string? PlanName,
    decimal? PlanPrice,
    string Status,
    bool IsActive,
    DateTime StartDate,
    DateTime? EndDate
);
