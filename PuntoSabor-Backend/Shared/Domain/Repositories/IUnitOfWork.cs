namespace PuntoSabor_Backend.Shared.Domain.Repositories;

/**
 * <summary>
 *     Maneja la confirmación de cambios en la transacción actual.
 * </summary>
 */

public interface IUnitOfWork
{
    Task CompleteAsync(CancellationToken ct = default);
}