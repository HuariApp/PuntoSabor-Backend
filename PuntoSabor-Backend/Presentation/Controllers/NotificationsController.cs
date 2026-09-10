using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using PuntoSabor_Backend.Notifications.Domain.Repositories;
using PuntoSabor_Backend.Presentation.Resources;
using PuntoSabor_Backend.Shared.Domain.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace PuntoSabor_Backend.Presentation.Controllers;

/**
 * <summary>
 *     Notificaciones del usuario (US12). El dueño recibe avisos de nuevas
 *     reseñas; aquí se listan y se marcan como leídas.
 * </summary>
 */
[ApiController]
[Route("notifications")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Notification Endpoints.")]
public class NotificationsController(
    INotificationRepository notifications,
    IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation("List Notifications", "Returns the notifications for a user.", OperationId = "ListNotifications")]
    [SwaggerResponse(200, "Notificaciones.", typeof(IEnumerable<NotificationResource>))]
    [SwaggerResponse(400, "Parámetro inválido.", typeof(ErrorResource))]
    public async Task<IActionResult> List([FromQuery] int userId, CancellationToken ct)
    {
        if (userId <= 0)
            return BadRequest(new ErrorResource("El parámetro 'userId' debe ser mayor a 0."));

        var result = await notifications.FindByUserIdAsync(userId, ct);
        return Ok(result.Select(n =>
            new NotificationResource(n.Id, n.UserId, n.Title, n.Body, n.IsRead, n.CreatedAt)));
    }

    [HttpPatch("{id:int}/read")]
    [SwaggerOperation("Mark Notification Read", "Marks a notification as read.", OperationId = "MarkNotificationRead")]
    [SwaggerResponse(204, "Notificación marcada como leída.")]
    [SwaggerResponse(404, "Notificación no encontrada.", typeof(ErrorResource))]
    public async Task<IActionResult> MarkRead(int id, CancellationToken ct)
    {
        var notification = await notifications.FindByIdAsync(id, ct);
        if (notification is null)
            return NotFound(new ErrorResource("Notificación no encontrada."));

        notification.IsRead = true;
        notification.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.CompleteAsync(ct);
        return NoContent();
    }
}
