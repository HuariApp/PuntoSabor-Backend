using PuntoSabor_Backend.Promotions.Domain.Model;
using PuntoSabor_Backend.Presentation.Resources;

namespace PuntoSabor_Backend.Presentation.Transform;

public static class PromoResourceFromEntityAssembler
{
    public static PromoResource ToResourceFromEntity(Promo entity) =>
        new(entity.Id, entity.Title, entity.Note, entity.Type, entity.Discount,
            entity.Code, entity.StartDate, entity.EndDate, entity.MaxUses,
            entity.CurrentUses, entity.HuariqueId, entity.ImageUrl,
            entity.IsActive, entity.CreatedAt, entity.UpdatedAt);
}
