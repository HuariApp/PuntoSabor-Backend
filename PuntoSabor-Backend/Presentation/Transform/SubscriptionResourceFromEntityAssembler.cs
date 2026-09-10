using PuntoSabor_Backend.Memberships.Domain.Model;
using PuntoSabor_Backend.Presentation.Resources;

namespace PuntoSabor_Backend.Presentation.Transform;

public static class SubscriptionResourceFromEntityAssembler
{
    public static SubscriptionResource ToResourceFromEntity(Subscription entity) => new(
        entity.Id,
        entity.UserId,
        entity.PlanId,
        entity.Plan?.Name,
        entity.Plan?.Price,
        entity.Status,
        entity.IsActive,
        entity.StartDate,
        entity.EndDate
    );
}
