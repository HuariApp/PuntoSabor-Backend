using PuntoSabor_Backend.Favorites.Domain.Model;

namespace PuntoSabor_Backend.Favorites.Domain.Repositories;

public interface IFavoriteRepository
{
    Task<IEnumerable<Favorite>> FindByUserIdAsync(int userId, CancellationToken ct = default);
    Task<Favorite?> FindByUserAndHuariqueAsync(int userId, int huariqueId, CancellationToken ct = default);
    Task AddAsync(Favorite favorite, CancellationToken ct = default);
    void Remove(Favorite favorite);
}
