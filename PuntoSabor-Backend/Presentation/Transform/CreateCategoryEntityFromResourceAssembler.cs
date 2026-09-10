using PuntoSabor_Backend.Discovery.Domain.Model;
using PuntoSabor_Backend.Presentation.Resources;

namespace PuntoSabor_Backend.Presentation.Transform;

public static class CreateCategoryEntityFromResourceAssembler
{
    public static Category ToEntityFromResource(CreateCategoryResource resource)
        => new()
        {
            Name = resource.Name.Trim()
        };
}
