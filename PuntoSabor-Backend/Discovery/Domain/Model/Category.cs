using PuntoSabor_Backend.Shared.Domain.Model;

namespace PuntoSabor_Backend.Discovery.Domain.Model;

/**
 * <summary>
 *     Representa una categoría de huariques dentro del módulo Discovery.
 *     Cada categoría hereda propiedades auditables como Id, CreatedAt y UpdatedAt.
 * </summary>
 */

public class Category : AuditableEntity
{
    public string Name { get; set; } = null!;
}