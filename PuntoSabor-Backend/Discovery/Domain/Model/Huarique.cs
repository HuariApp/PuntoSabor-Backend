using PuntoSabor_Backend.Shared.Domain.Model;

namespace PuntoSabor_Backend.Discovery.Domain.Model;

/// <summary>
/// Represents a local food establishment registered in the PuntoSabor platform.
/// </summary>
public class Huarique : AuditableEntity
{
    /// <summary>
    /// Display name of the huarique shown to explorers.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Food category associated with this huarique.
    /// </summary>
    public string Category { get; set; } = null!;
    public int CategoryId { get; set; }
    
    public decimal Price { get; set; }
    
    public double Rating { get; set; }
    
    public string District { get; set; } = null!;

    public bool Near { get; set; }

    // Coordenadas geográficas — opcionales, usadas para mapas y rutas
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public int? OwnerId { get; set; }

    // Campos de detalle — opcionales, gestionados por el propietario
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    // Imagen guardada directamente en la base de datos (LONGBLOB)
    public byte[]? ImageData { get; set; }
    public string? ImageContentType { get; set; }
    public string? OpenAt { get; set; }
    public string? CloseAt { get; set; }
    public bool DeliveryAvailable { get; set; } = false;
    public bool TakeawayAvailable { get; set; } = false;
    public bool DineInAvailable { get; set; } = true;
}