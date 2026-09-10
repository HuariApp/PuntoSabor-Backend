using Microsoft.EntityFrameworkCore;
using PuntoSabor_Backend.Auth.Domain.Model;
using PuntoSabor_Backend.Auth.Domain.Repositories;
using PuntoSabor_Backend.Shared.Infrastructure.Persistence.EFC;

namespace PuntoSabor_Backend.Auth.Infrastructure.Persistence.EFC.Repositories;

/**
 * <summary>
 *     Implementación del repositorio de usuarios utilizando Entity Framework Core.
 *     Permite buscar usuarios por correo y registrar nuevos usuarios en la base de datos.
 * </summary>
 */

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<IEnumerable<User>> FindByEmailAsync(
        string? email,
        CancellationToken ct = default)
    
    {
        var query = context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(email))
            
        {
            query = query.Where(u => u.Email == email);
        }

        return await query.AsNoTracking().ToListAsync(ct);
    }

    public async Task<User?> FindByIdAsync(int id, CancellationToken ct = default)
        => await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> FindTrackedByEmailAsync(string email, CancellationToken ct = default)
        => await context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<User?> FindTrackedByIdAsync(int id, CancellationToken ct = default)
        => await context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await context.Users.AddAsync(user, ct);
    }

    public void Remove(User user)
    {
        context.Users.Remove(user);
    }
}