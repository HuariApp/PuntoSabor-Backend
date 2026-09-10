using Microsoft.EntityFrameworkCore;
using PuntoSabor_Backend.Notifications.Domain.Model;
using PuntoSabor_Backend.Notifications.Domain.Repositories;
using PuntoSabor_Backend.Shared.Infrastructure.Persistence.EFC;

namespace PuntoSabor_Backend.Notifications.Infrastructure.Persistence.EFC.Repositories;

public class NotificationRepository(AppDbContext context) : INotificationRepository
{
    public async Task<IEnumerable<Notification>> FindByUserIdAsync(int userId, CancellationToken ct = default)
        => await context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .AsNoTracking()
            .ToListAsync(ct);

    // Tracked: se usa para marcar como leída.
    public async Task<Notification?> FindByIdAsync(int id, CancellationToken ct = default)
        => await context.Notifications.FirstOrDefaultAsync(n => n.Id == id, ct);

    public async Task AddAsync(Notification notification, CancellationToken ct = default)
        => await context.Notifications.AddAsync(notification, ct);
}
