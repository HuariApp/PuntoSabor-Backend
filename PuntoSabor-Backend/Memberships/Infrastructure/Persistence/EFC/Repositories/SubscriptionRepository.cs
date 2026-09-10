using Microsoft.EntityFrameworkCore;
using PuntoSabor_Backend.Memberships.Domain.Model;
using PuntoSabor_Backend.Memberships.Domain.Repositories;
using PuntoSabor_Backend.Shared.Infrastructure.Persistence.EFC;

namespace PuntoSabor_Backend.Memberships.Infrastructure.Persistence.EFC.Repositories;

public class SubscriptionRepository(AppDbContext context) : ISubscriptionRepository
{
    public async Task<IEnumerable<Subscription>> FindByUserIdAsync(int userId, CancellationToken ct = default)
        => await context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartDate)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<Subscription?> FindActiveByUserIdAsync(int userId, CancellationToken ct = default)
        => await context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.UserId == userId && s.Status == "active")
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefaultAsync(ct);

    public async Task<Subscription?> FindByIdAsync(int id, CancellationToken ct = default)
        => await context.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task AddAsync(Subscription entity, CancellationToken ct = default)
        => await context.Subscriptions.AddAsync(entity, ct);

    public void Remove(Subscription entity)
        => context.Subscriptions.Remove(entity);
}
