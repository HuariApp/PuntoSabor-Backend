using PuntoSabor_Backend.Memberships.Domain.Model;

namespace PuntoSabor_Backend.Memberships.Domain.Repositories;

/**
 * <summary>
 *     Repositorio para obtener la lista de planes de membresía.
 * </summary>
 */

public interface IPlanRepository
{
    Task<IEnumerable<Plan>> ListAsync(CancellationToken ct = default);
}