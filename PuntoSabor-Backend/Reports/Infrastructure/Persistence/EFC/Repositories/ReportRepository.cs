using Microsoft.EntityFrameworkCore;
using PuntoSabor_Backend.Reports.Domain.Model;
using PuntoSabor_Backend.Reports.Domain.Repositories;
using PuntoSabor_Backend.Shared.Infrastructure.Persistence.EFC;

namespace PuntoSabor_Backend.Reports.Infrastructure.Persistence.EFC.Repositories;

public class ReportRepository(AppDbContext context) : IReportRepository
{
    public async Task<IEnumerable<Report>> FindByHuariqueIdAsync(int? huariqueId, CancellationToken ct = default)
    {
        var query = context.Reports.AsQueryable();
        if (huariqueId is not null)
            query = query.Where(r => r.HuariqueId == huariqueId);
        return await query.AsNoTracking().OrderByDescending(r => r.CreatedAt).ToListAsync(ct);
    }

    public async Task AddAsync(Report report, CancellationToken ct = default)
        => await context.Reports.AddAsync(report, ct);
}
