using PuntoSabor_Backend.Promotions.Domain.Model;
using PuntoSabor_Backend.Presentation.Resources;

namespace PuntoSabor_Backend.Presentation.Transform;

public static class CreatePromoEntityFromResourceAssembler
{
    public static Promo ToEntityFromResource(CreatePromoResource resource) =>
        new()
        {
            Title = resource.Title.Trim(),
            Note = resource.Note.Trim(),
            HuariqueId = resource.HuariqueId,
            Type = resource.Type.Trim().ToLowerInvariant(),
            Discount = resource.Discount,
            Code = resource.Code?.Trim(),
            StartDate = resource.StartDate,
            EndDate = resource.EndDate,
            MaxUses = resource.MaxUses,
            ImageUrl = resource.ImageUrl?.Trim()
        };
}
