using Microsoft.AspNetCore.Mvc;
using PuntoSabor_Backend.Discovery.Domain.Repositories;
using PuntoSabor_Backend.Notifications.Domain.Model;
using PuntoSabor_Backend.Notifications.Domain.Repositories;
using PuntoSabor_Backend.Presentation.Resources;
using PuntoSabor_Backend.Reviews.Domain.Model;
using PuntoSabor_Backend.Reviews.Domain.Repositories;
using PuntoSabor_Backend.Shared.Domain.Repositories;

namespace PuntoSabor_Backend.Presentation.Controllers;

[ApiController]
[Route("reviews")]
public class ReviewsController(
    IReviewRepository reviews,
    IHuariqueRepository huariques,
    INotificationRepository notifications,
    IUnitOfWork unitOfWork) : ControllerBase
{
    // Lista mínima de moderación de contenido ofensivo (US08).
    private static readonly string[] BannedWords =
    {
        "idiota", "imbecil", "imbécil", "estupido", "estúpido", "mierda",
        "puta", "puto", "basura", "asco", "maldito", "pendejo", "cabron", "cabrón"
    };
    /**
     * <summary>
     *     Lista de reviews.
     *     GET /reviews
     *     GET /reviews?huariqueId=1&_sort=createdAt&_order=desc
     * </summary>
     */
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Review>>> GetAll(
        [FromQuery] int? huariqueId,
        [FromQuery(Name = "_sort")] string? sort,
        [FromQuery(Name = "_order")] string? order,
        CancellationToken ct)
    {
        var result = await reviews.ListAsync(huariqueId, sort, order, ct);
        return Ok(result);
    }

    /**
     * <summary>
     *     Crea una nueva review.
     *     POST /reviews
     * </summary>
     */
    [HttpPost]
    public async Task<ActionResult<Review>> Create(
        [FromBody] Review dto,
        CancellationToken ct)
    {
        // Moderación de reseñas inapropiadas (US08): se bloquea la publicación
        // cuando el comentario contiene lenguaje ofensivo.
        if (!string.IsNullOrWhiteSpace(dto.Comment) && ContainsOffensiveLanguage(dto.Comment))
            return BadRequest(new ErrorResource(
                "Tu reseña contiene lenguaje inapropiado. Edítala para poder publicarla."));

        if (dto.CreatedAt == default)
            dto.CreatedAt = DateTime.UtcNow;
        if (dto.CreatedAtReview == default)
            dto.CreatedAtReview = DateTime.UtcNow;

        await reviews.AddAsync(dto, ct);
        await unitOfWork.CompleteAsync(ct);

        // Notifica al dueño del huarique sobre la nueva reseña (US12).
        await NotifyOwnerAsync(dto, ct);

        return StatusCode(StatusCodes.Status201Created, dto);
    }

    private static bool ContainsOffensiveLanguage(string comment)
    {
        var lower = comment.ToLowerInvariant();
        return BannedWords.Any(w => lower.Contains(w));
    }

    private async Task NotifyOwnerAsync(Review review, CancellationToken ct)
    {
        try
        {
            var huarique = await huariques.FindByIdAsync(review.HuariqueId, ct);
            var ownerId = huarique?.OwnerId ?? 0;
            if (ownerId <= 0 || ownerId == review.UserId) return;

            await notifications.AddAsync(new Notification
            {
                UserId = ownerId,
                Title = "Nueva reseña en tu huarique",
                Body = $"{huarique!.Name} recibió una reseña de {review.Rating}★."
            }, ct);
            await unitOfWork.CompleteAsync(ct);
        }
        catch
        {
            // La notificación es complementaria: no debe afectar la creación de la reseña.
        }
    }
}