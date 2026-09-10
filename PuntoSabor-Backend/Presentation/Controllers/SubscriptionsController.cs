using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using PuntoSabor_Backend.Memberships.Domain.Repositories;
using PuntoSabor_Backend.Presentation.Resources;
using PuntoSabor_Backend.Presentation.Transform;
using PuntoSabor_Backend.Shared.Domain.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace PuntoSabor_Backend.Presentation.Controllers;

[ApiController]
[Route("subscriptions")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Subscriptions Endpoints.")]
public class SubscriptionsController(
    ISubscriptionRepository subscriptions,
    IPlanRepository plans,
    IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation("List Subscriptions", "Returns all subscriptions for a user.", OperationId = "ListSubscriptions")]
    [SwaggerResponse(200, "Lista de suscripciones.", typeof(IEnumerable<SubscriptionResource>))]
    public async Task<IActionResult> List([FromQuery] int userId, CancellationToken ct)
    {
        var result = await subscriptions.FindByUserIdAsync(userId, ct);
        return Ok(result.Select(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("active")]
    [SwaggerOperation("Get Active Subscription", "Returns the active subscription for a user.", OperationId = "GetActiveSubscription")]
    [SwaggerResponse(200, "Suscripción activa.", typeof(SubscriptionResource))]
    [SwaggerResponse(404, "Sin suscripción activa.", typeof(ErrorResource))]
    public async Task<IActionResult> GetActive([FromQuery] int userId, CancellationToken ct)
    {
        var sub = await subscriptions.FindActiveByUserIdAsync(userId, ct);
        if (sub is null) return NotFound(new ErrorResource("Sin suscripción activa."));
        return Ok(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(sub));
    }

    /**
     * <summary>
     *     Comprobante de pago de una suscripción (US25).
     *     GET /subscriptions/:id/receipt
     * </summary>
     */
    [HttpGet("{id:int}/receipt")]
    [SwaggerOperation("Get Subscription Receipt", "Returns a downloadable receipt for a subscription payment.", OperationId = "GetSubscriptionReceipt")]
    [SwaggerResponse(200, "Comprobante.", typeof(ReceiptResource))]
    [SwaggerResponse(404, "Suscripción no encontrada.", typeof(ErrorResource))]
    public async Task<IActionResult> GetReceipt(int id, CancellationToken ct)
    {
        var sub = await subscriptions.FindByIdAsync(id, ct);
        if (sub is null) return NotFound(new ErrorResource("Suscripción no encontrada."));

        var receipt = new ReceiptResource(
            ReceiptNumber: $"PS-{sub.Id:D6}",
            SubscriptionId: sub.Id,
            UserId: sub.UserId,
            PlanId: sub.PlanId,
            PlanName: sub.Plan?.Name ?? sub.PlanId,
            Amount: sub.Plan?.Price ?? 0m,
            Currency: "PEN",
            Status: sub.Status,
            IssuedAt: sub.StartDate,
            PeriodStart: sub.StartDate,
            PeriodEnd: sub.EndDate
        );
        return Ok(receipt);
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation("Get Subscription", "Returns a single subscription by ID.", OperationId = "GetSubscription")]
    [SwaggerResponse(200, "Suscripción.", typeof(SubscriptionResource))]
    [SwaggerResponse(404, "No encontrada.", typeof(ErrorResource))]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var sub = await subscriptions.FindByIdAsync(id, ct);
        if (sub is null) return NotFound(new ErrorResource("Suscripción no encontrada."));
        return Ok(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(sub));
    }

    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [SwaggerOperation("Subscribe", "Subscribes a user to a plan, cancelling any previous active subscription.", OperationId = "Subscribe")]
    [SwaggerResponse(201, "Suscripción creada.", typeof(SubscriptionResource))]
    [SwaggerResponse(400, "Datos inválidos.", typeof(ErrorResource))]
    [SwaggerResponse(404, "Plan no encontrado.", typeof(ErrorResource))]
    public async Task<IActionResult> Subscribe([FromBody] CreateSubscriptionResource resource, CancellationToken ct)
    {
        if (resource is null)
            return BadRequest(new ErrorResource("Body inválido."));

        if (resource.UserId <= 0)
            return BadRequest(new ErrorResource("UserId inválido."));

        var planId = resource.PlanId?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(planId))
            return BadRequest(new ErrorResource("PlanId es obligatorio."));

        var allPlans = await plans.ListAsync(ct);
        if (!allPlans.Any(p => p.Id == planId))
            return NotFound(new ErrorResource($"Plan '{planId}' no encontrado."));

        // Cancel previous active subscription
        var current = await subscriptions.FindActiveByUserIdAsync(resource.UserId, ct);
        if (current is not null)
        {
            if (current.PlanId == planId)
                return BadRequest(new ErrorResource("El usuario ya tiene este plan activo."));

            current.Status = "cancelled";
            current.UpdatedAt = DateTime.UtcNow;
        }

        var entity = CreateSubscriptionEntityFromResourceAssembler.ToEntityFromResource(resource);
        await subscriptions.AddAsync(entity, ct);
        await unitOfWork.CompleteAsync(ct);

        // Reload to get Plan navigation property
        var saved = await subscriptions.FindByIdAsync(entity.Id, ct);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id },
            SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(saved!));
    }

    [HttpPost("{id:int}/cancel")]
    [SwaggerOperation("Cancel Subscription", "Cancels an active subscription.", OperationId = "CancelSubscription")]
    [SwaggerResponse(200, "Suscripción cancelada.", typeof(SubscriptionResource))]
    [SwaggerResponse(400, "La suscripción no está activa.", typeof(ErrorResource))]
    [SwaggerResponse(404, "No encontrada.", typeof(ErrorResource))]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var sub = await subscriptions.FindByIdAsync(id, ct);
        if (sub is null) return NotFound(new ErrorResource("Suscripción no encontrada."));

        if (sub.Status != "active")
            return BadRequest(new ErrorResource("Esta suscripción ya no está activa."));

        sub.Status = "cancelled";
        sub.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.CompleteAsync(ct);
        return Ok(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(sub));
    }

    [HttpDelete("{id:int}")]
    [SwaggerOperation("Delete Subscription", "Permanently deletes a subscription record.", OperationId = "DeleteSubscription")]
    [SwaggerResponse(204, "Eliminada.")]
    [SwaggerResponse(404, "No encontrada.", typeof(ErrorResource))]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var sub = await subscriptions.FindByIdAsync(id, ct);
        if (sub is null) return NotFound(new ErrorResource("Suscripción no encontrada."));

        subscriptions.Remove(sub);
        await unitOfWork.CompleteAsync(ct);
        return NoContent();
    }
}
