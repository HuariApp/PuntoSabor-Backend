using PuntoSabor_Backend.Memberships.Domain.Model;

namespace PuntoSabor_Backend.Memberships.Domain.Repositories;

public interface ISubscriptionRepository
{
    Task<IEnumerable<Subscription>> FindByUserIdAsync(int userId, CancellationToken ct = default);
    Task<Subscription?> FindActiveByUserIdAsync(int userId, CancellationToken ct = default);
    Task<Subscription?> FindByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Subscription entity, CancellationToken ct = default);
    void Remove(Subscription entity);
}
