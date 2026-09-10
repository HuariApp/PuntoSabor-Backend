using Microsoft.EntityFrameworkCore;
using PuntoSabor_Backend.Shared.Infrastructure.Persistence.EFC;
using PuntoSabor_Backend.UserPreferences.Domain.Model;
using PuntoSabor_Backend.UserPreferences.Domain.Repositories;

namespace PuntoSabor_Backend.UserPreferences.Infrastructure.Persistence.EFC.Repositories;

public class UserPreferenceRepository(AppDbContext context) : IUserPreferenceRepository
{
    // Tracked: la entidad se actualiza al guardar las preferencias.
    public async Task<UserPreference?> FindByUserIdAsync(int userId, CancellationToken ct = default)
        => await context.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId, ct);

    public async Task AddAsync(UserPreference preference, CancellationToken ct = default)
        => await context.UserPreferences.AddAsync(preference, ct);
}
