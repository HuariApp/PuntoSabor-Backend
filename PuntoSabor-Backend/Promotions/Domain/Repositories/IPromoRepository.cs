using PuntoSabor_Backend.Promotions.Domain.Model;

namespace PuntoSabor_Backend.Promotions.Domain.Repositories;

public interface IPromoRepository
{
    Task<IEnumerable<Promo>> ListAsync(CancellationToken ct = default);
    Task<IEnumerable<Promo>> FindByHuariqueIdAsync(int huariqueId, CancellationToken ct = default);
    Task<IEnumerable<Promo>> FindByOwnerIdAsync(int ownerId, CancellationToken ct = default);
    Task<Promo?> FindByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Promo promo, CancellationToken ct = default);
    void Remove(Promo promo);
}
