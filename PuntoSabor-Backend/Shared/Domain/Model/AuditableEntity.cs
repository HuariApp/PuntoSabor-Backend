namespace PuntoSabor_Backend.Shared.Domain.Model;

/**
 * <summary>
 *     Entidad base con Id y marcas de creación y actualización.
 * </summary>
 */

public abstract class AuditableEntity
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
}