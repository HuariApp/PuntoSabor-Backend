using PuntoSabor_Backend.Auth.Domain.Model;
using PuntoSabor_Backend.Presentation.Resources;

namespace PuntoSabor_Backend.Presentation.Transform;

/**
 * <summary>
 *     Convierte una entidad User en su recurso de respuesta.
 * </summary>
 */


public static class UserResourceFromEntityAssembler
{
    public static UserResource ToResourceFromEntity(User entity)
        => new(
            entity.Id,
            entity.Name,
            entity.Email,
            entity.Role.ToString().ToLowerInvariant(),
            entity.CreatedAt,
            entity.UpdatedAt
        );
}