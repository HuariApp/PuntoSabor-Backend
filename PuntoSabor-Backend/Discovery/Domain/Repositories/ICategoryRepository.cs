using PuntoSabor_Backend.Discovery.Domain.Model;

namespace PuntoSabor_Backend.Discovery.Domain.Repositories;

/**
 * <summary>
 *     Repositorio para obtener y gestionar las categorías disponibles.
 * </summary>
 */

public interface ICategoryRepository

{
    Task<IEnumerable<Category>> ListAsync(CancellationToken ct = default);

    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);

    Task AddAsync(Category category, CancellationToken ct = default);
}
