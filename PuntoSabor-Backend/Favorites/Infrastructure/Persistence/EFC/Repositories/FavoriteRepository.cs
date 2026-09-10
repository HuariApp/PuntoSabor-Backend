using Microsoft.EntityFrameworkCore;
using PuntoSabor_Backend.Favorites.Domain.Model;
using PuntoSabor_Backend.Favorites.Domain.Repositories;
using PuntoSabor_Backend.Shared.Infrastructure.Persistence.EFC;

namespace PuntoSabor_Backend.Favorites.Infrastructure.Persistence.EFC.Repositories;

public class FavoriteRepository(AppDbContext context) : IFavoriteRepository
{
    public async Task<IEnumerable<Favorite>> FindByUserIdAsync(int userId, CancellationToken ct = default)
        => await context.Favorites
            .Where(f => f.UserId == userId)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<Favorite?> FindByUserAndHuariqueAsync(int userId, int huariqueId, CancellationToken ct = default)
        => await context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.HuariqueId == huariqueId, ct);

    public async Task AddAsync(Favorite favorite, CancellationToken ct = default)
        => await context.Favorites.AddAsync(favorite, ct);

    public void Remove(Favorite favorite)
        => context.Favorites.Remove(favorite);
}
