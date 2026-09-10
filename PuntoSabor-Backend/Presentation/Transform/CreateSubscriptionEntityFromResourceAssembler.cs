using PuntoSabor_Backend.Memberships.Domain.Model;
using PuntoSabor_Backend.Presentation.Resources;

namespace PuntoSabor_Backend.Presentation.Transform;

public static class CreateSubscriptionEntityFromResourceAssembler
{
    public static Subscription ToEntityFromResource(CreateSubscriptionResource resource) => new()
    {
        UserId    = resource.UserId,
        PlanId    = resource.PlanId.Trim().ToLowerInvariant(),
        StartDate = DateTime.UtcNow,
        EndDate   = resource.EndDate,
        Status    = "active"
    };
}
