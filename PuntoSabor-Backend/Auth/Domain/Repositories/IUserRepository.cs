using PuntoSabor_Backend.Auth.Domain.Model;

namespace PuntoSabor_Backend.Auth.Domain.Repositories;

/**
 * <summary>
 *     Repositorio para gestionar usuarios y realizar búsquedas por correo.
 * </summary>
 */

public interface IUserRepository
{
    
    Task<IEnumerable<User>> FindByEmailAsync(
        string? email,
        CancellationToken ct = default);

    Task<User?> FindByIdAsync(int id, CancellationToken ct = default);

    /// Búsqueda rastreada por email para poder actualizar la entidad (US16).
    Task<User?> FindTrackedByEmailAsync(string email, CancellationToken ct = default);

    /// Búsqueda rastreada por id para poder actualizar o eliminar la entidad (CRUD de perfil).
    Task<User?> FindTrackedByIdAsync(int id, CancellationToken ct = default);

    Task AddAsync(
        User user,
        CancellationToken ct = default);

    void Remove(User user);
}