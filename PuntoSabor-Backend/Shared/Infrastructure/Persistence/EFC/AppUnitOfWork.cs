using PuntoSabor_Backend.Shared.Domain.Repositories;

namespace PuntoSabor_Backend.Shared.Infrastructure.Persistence.EFC;

/**
 * <summary>
 *     Unit of Work para guardar cambios en la base de datos.
 * </summary>
 */

public class AppUnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task CompleteAsync(CancellationToken ct = default)
    
    {
        await context.SaveChangesAsync(ct);
    }
}