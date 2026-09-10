using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using PuntoSabor_Backend.Presentation.Resources;
using PuntoSabor_Backend.Reports.Domain.Model;
using PuntoSabor_Backend.Reports.Domain.Repositories;
using PuntoSabor_Backend.Shared.Domain.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace PuntoSabor_Backend.Presentation.Controllers;

/**
 * <summary>
 *     Reportes de información incorrecta de huariques (US21).
 * </summary>
 */
[ApiController]
[Route("reports")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Report Endpoints.")]
public class ReportsController(
    IReportRepository reports,
    IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation("List Reports", "Returns reports, optionally filtered by huarique.", OperationId = "ListReports")]
    [SwaggerResponse(200, "Reportes.", typeof(IEnumerable<ReportResource>))]
    public async Task<IActionResult> List([FromQuery] int? huariqueId, CancellationToken ct)
    {
        var result = await reports.FindByHuariqueIdAsync(huariqueId, ct);
        return Ok(result.Select(ToResource));
    }

    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [SwaggerOperation("Create Report", "Reports incorrect information about a huarique.", OperationId = "CreateReport")]
    [SwaggerResponse(201, "Reporte registrado.", typeof(ReportResource))]
    [SwaggerResponse(400, "Datos inválidos.", typeof(ErrorResource))]
    public async Task<IActionResult> Create([FromBody] CreateReportResource resource, CancellationToken ct)
    {
        if (resource is null)
            return BadRequest(new ErrorResource("Body inválido."));
        if (resource.HuariqueId <= 0)
            return BadRequest(new ErrorResource("El campo 'huariqueId' debe ser mayor a 0."));
        if (resource.UserId <= 0)
            return BadRequest(new ErrorResource("El campo 'userId' debe ser mayor a 0."));
        if (string.IsNullOrWhiteSpace(resource.Reason))
            return BadRequest(new ErrorResource("El campo 'reason' es obligatorio."));

        var report = new Report
        {
            HuariqueId = resource.HuariqueId,
            UserId = resource.UserId,
            Reason = resource.Reason.Trim(),
            Status = "pending"
        };

        await reports.AddAsync(report, ct);
        await unitOfWork.CompleteAsync(ct);

        return StatusCode(StatusCodes.Status201Created, ToResource(report));
    }

    private static ReportResource ToResource(Report r) =>
        new(r.Id, r.HuariqueId, r.UserId, r.Reason, r.Status, r.CreatedAt);
}
