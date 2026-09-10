using PuntoSabor_Backend.Notifications.Domain.Model;

namespace PuntoSabor_Backend.Notifications.Domain.Repositories;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> FindByUserIdAsync(int userId, CancellationToken ct = default);
    Task<Notification?> FindByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Notification notification, CancellationToken ct = default);
}
