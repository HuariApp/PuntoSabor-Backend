namespace PuntoSabor_Backend.Presentation.Resources;

public record CreateSubscriptionResource(
    int UserId,
    string PlanId,
    DateTime? EndDate = null
);
