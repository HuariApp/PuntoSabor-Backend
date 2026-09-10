using Microsoft.EntityFrameworkCore;
using PuntoSabor_Backend.Reviews.Domain.Model;
using PuntoSabor_Backend.Reviews.Domain.Repositories;
using PuntoSabor_Backend.Shared.Infrastructure.Persistence.EFC;

namespace PuntoSabor_Backend.Reviews.Infrastructure.Persistence.EFC.Repositories;

/**
 * <summary>
 *     Implementación del repositorio de reseñas con filtros y ordenamiento.
 * </summary>
 */

public class ReviewRepository(AppDbContext context) : IReviewRepository
{
    
    public async Task<IEnumerable<Review>> ListAsync(
        int? huariqueId,
        string? sort,
        string? order,
        CancellationToken ct = default)
    
    {
        var query = context.Reviews.AsQueryable();

        if (huariqueId.HasValue)
            query = query.Where(r => r.HuariqueId == huariqueId.Value);

        if (sort == "createdAt")
            query = (order?.ToLower() == "desc")
                ? query.OrderByDescending(r => r.CreatedAt)
                : query.OrderBy(r => r.CreatedAt);

        return await query.AsNoTracking().ToListAsync(ct);
    }

    public async Task AddAsync(Review review, CancellationToken ct = default)
    {
        await context.Reviews.AddAsync(review, ct);
    }
}