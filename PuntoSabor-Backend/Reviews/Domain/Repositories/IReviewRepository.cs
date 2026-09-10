using PuntoSabor_Backend.Reviews.Domain.Model;

namespace PuntoSabor_Backend.Reviews.Domain.Repositories;

public interface IReviewRepository
{
    /**
 * <summary>
 *     Lista las reviews, opcionalmente filtrando por huariqueId
 *     y permitiendo sort por createdAt (asc/desc).
 * </summary>
 */
    Task<IEnumerable<Review>> ListAsync(
        int? huariqueId,
        string? sort,
        string? order,
        CancellationToken ct = default);

    /**
     * <summary>
     *     Agrega una nueva review al contexto.
     * </summary>
     */
    Task AddAsync(Review review, CancellationToken ct = default);
}