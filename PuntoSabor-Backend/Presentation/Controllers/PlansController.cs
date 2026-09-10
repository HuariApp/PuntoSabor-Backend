using Microsoft.AspNetCore.Mvc;
using PuntoSabor_Backend.Memberships.Domain.Model;
using PuntoSabor_Backend.Memberships.Domain.Repositories;

namespace PuntoSabor_Backend.Presentation.Controllers;

[ApiController]
[Route("plans")]
public class PlansController(IPlanRepository plans) : ControllerBase
{
    /**
     * <summary>
     *     Devuelve la lista de planes de membresía.
     *     GET /plans
     * </summary>
     */
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Plan>>> GetAll(CancellationToken ct)
    {
        var result = await plans.ListAsync(ct);
        return Ok(result);
    }
}