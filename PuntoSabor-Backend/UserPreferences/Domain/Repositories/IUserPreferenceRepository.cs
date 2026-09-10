using PuntoSabor_Backend.UserPreferences.Domain.Model;

namespace PuntoSabor_Backend.UserPreferences.Domain.Repositories;

public interface IUserPreferenceRepository
{
    Task<UserPreference?> FindByUserIdAsync(int userId, CancellationToken ct = default);
    Task AddAsync(UserPreference preference, CancellationToken ct = default);
}
