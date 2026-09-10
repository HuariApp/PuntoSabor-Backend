using PuntoSabor_Backend.Discovery.Domain.Model;
using PuntoSabor_Backend.Presentation.Resources;

namespace PuntoSabor_Backend.Presentation.Transform;

public static class CreateHuariqueEntityFromResourceAssembler
{
    public static Huarique ToEntityFromResource(CreateHuariqueResource resource)
        => new()
        {
            Name = resource.Name.Trim(),
            Category = resource.Category.Trim(),
            CategoryId = resource.CategoryId,
            Price = resource.Price,
            District = resource.District.Trim(),
            Rating = 0,
            Near = false,
            Latitude = resource.Latitude,
            Longitude = resource.Longitude,
            OwnerId = resource.OwnerId,
            Address = resource.Address?.Trim(),
            Phone = resource.Phone?.Trim(),
            Description = resource.Description?.Trim(),
            ImageUrl = resource.ImageUrl?.Trim(),
            OpenAt = resource.OpenAt?.Trim(),
            CloseAt = resource.CloseAt?.Trim(),
            DeliveryAvailable = resource.DeliveryAvailable,
            TakeawayAvailable = resource.TakeawayAvailable,
            DineInAvailable = resource.DineInAvailable
        };
}
