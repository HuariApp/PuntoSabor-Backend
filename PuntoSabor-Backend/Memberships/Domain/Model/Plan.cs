namespace PuntoSabor_Backend.Memberships.Domain.Model;

/**
 * <summary>
 *     Plan de membresía con identificador, nombre y precio.
 * </summary>
 */

public class Plan
{
    public string Id { get; set; } = null!;
    
    public string Name { get; set; } = null!;
    
    public decimal Price { get; set; }
    
}