using Microsoft.EntityFrameworkCore;
using PuntoSabor_Backend.Promotions.Domain.Model;
using PuntoSabor_Backend.Promotions.Domain.Repositories;
using PuntoSabor_Backend.Shared.Infrastructure.Persistence.EFC;

namespace PuntoSabor_Backend.Promotions.Infrastructure.Persistence.EFC.Repositories;

public class PromoRepository(AppDbContext context) : IPromoRepository
{
    public async Task<IEnumerable<Promo>> ListAsync(CancellationToken ct = default)
        => await context.Promos.AsNoTracking().ToListAsync(ct);

    public async Task<IEnumerable<Promo>> FindByHuariqueIdAsync(int huariqueId, CancellationToken ct = default)
        => await context.Promos
            .Where(p => p.HuariqueId == huariqueId)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<IEnumerable<Promo>> FindByOwnerIdAsync(int ownerId, CancellationToken ct = default)
    {
        var ownerHuariqueIds = await context.Huariques
            .Where(h => h.OwnerId == ownerId)
            .Select(h => h.Id)
            .ToListAsync(ct);

        return await context.Promos
            .Where(p => p.HuariqueId != null && ownerHuariqueIds.Contains(p.HuariqueId.Value))
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<Promo?> FindByIdAsync(int id, CancellationToken ct = default)
        => await context.Promos.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task AddAsync(Promo promo, CancellationToken ct = default)
        => await context.Promos.AddAsync(promo, ct);

    public void Remove(Promo promo)
        => context.Promos.Remove(promo);
}
