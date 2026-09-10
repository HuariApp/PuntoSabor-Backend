using Microsoft.EntityFrameworkCore;
using PuntoSabor_Backend.Discovery.Domain.Model;
using PuntoSabor_Backend.Discovery.Domain.Repositories;
using PuntoSabor_Backend.Shared.Infrastructure.Persistence.EFC;

namespace PuntoSabor_Backend.Discovery.Infrastructure.Persistence.EFC.Repositories;

/**
 * <summary>
 *     Implementación del repositorio de categorías utilizando Entity Framework Core.
 * </summary>
 */
public class CategoryRepository(AppDbContext context) : ICategoryRepository
{

    public async Task<IEnumerable<Category>> ListAsync(CancellationToken ct = default)

    {
        return await context.Categories.AsNoTracking().ToListAsync(ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        return await context.Categories
            .AsNoTracking()
            .AnyAsync(c => c.Name.ToLower() == name.ToLower(), ct);
    }

    public async Task AddAsync(Category category, CancellationToken ct = default)
    {
        await context.Categories.AddAsync(category, ct);
    }
}
