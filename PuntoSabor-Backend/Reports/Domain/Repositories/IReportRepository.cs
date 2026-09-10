using PuntoSabor_Backend.Reports.Domain.Model;

namespace PuntoSabor_Backend.Reports.Domain.Repositories;

public interface IReportRepository
{
    Task<IEnumerable<Report>> FindByHuariqueIdAsync(int? huariqueId, CancellationToken ct = default);
    Task AddAsync(Report report, CancellationToken ct = default);
}
