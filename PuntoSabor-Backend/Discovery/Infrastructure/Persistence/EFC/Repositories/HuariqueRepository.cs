using Microsoft.EntityFrameworkCore;
using PuntoSabor_Backend.Discovery.Domain.Model;
using PuntoSabor_Backend.Discovery.Domain.Repositories;
using PuntoSabor_Backend.Shared.Infrastructure.Persistence.EFC;

namespace PuntoSabor_Backend.Discovery.Infrastructure.Persistence.EFC.Repositories;

/**
 * <summary>
 *     Repositorio de huariques con búsqueda, consulta por id y actualización parcial.
 * </summary>
 */

public class HuariqueRepository(AppDbContext context) : IHuariqueRepository
{
    public async Task<IEnumerable<Huarique>> SearchAsync(
        string? q,
        bool? near,
        int? ownerId,
        CancellationToken ct = default)
    {
        var query = context.Huariques.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.ToLowerInvariant();
            query = query.Where(h =>
                h.Name.ToLower().Contains(term) ||
                h.Category.ToLower().Contains(term) ||
                h.District.ToLower().Contains(term));
        }

        if (near.HasValue && near.Value)
            query = query.Where(h => h.Near);

        if (ownerId.HasValue)
            query = query.Where(h => h.OwnerId == ownerId.Value);

        return await query.AsNoTracking().ToListAsync(ct);
    }

    public async Task<IEnumerable<Huarique>> FindByOwnerIdAsync(int ownerId, CancellationToken ct = default)
        => await context.Huariques
            .Where(h => h.OwnerId == ownerId)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<Huarique?> FindByIdAsync(int id, CancellationToken ct = default)
    
    {
        return await context.Huariques.FindAsync([id], ct);
    }

    public async Task AddAsync(Huarique huarique, CancellationToken ct = default)
    
    {
        await context.Huariques.AddAsync(huarique, ct);
    }

    public Task PatchAsync(Huarique huarique, IDictionary<string, object> patch, CancellationToken ct = default)
    
    {
        foreach (var (key, value) in patch)
        {
            switch (key.ToLowerInvariant())
            
            {
                case "name":       huarique.Name = value?.ToString() ?? huarique.Name; break;
                
                case "category":   huarique.Category = value?.ToString() ?? huarique.Category; break;
                
                case "categoryid": huarique.CategoryId = Convert.ToInt32(value); break;
                
                case "price":      huarique.Price = Convert.ToDecimal(value); break;
                
                case "rating":     huarique.Rating = Convert.ToDouble(value); break;
                
                case "district":   huarique.District = value?.ToString() ?? huarique.District; break;
                
                case "near":              huarique.Near = Convert.ToBoolean(value); break;
                case "latitude":          huarique.Latitude = value is null ? null : Convert.ToDouble(value); break;
                case "longitude":         huarique.Longitude = value is null ? null : Convert.ToDouble(value); break;
                case "ownerid":           huarique.OwnerId = value is null ? null : Convert.ToInt32(value); break;
                case "address":           huarique.Address = value?.ToString(); break;
                case "phone":             huarique.Phone = value?.ToString(); break;
                case "description":       huarique.Description = value?.ToString(); break;
                case "imageurl":          huarique.ImageUrl = value?.ToString(); break;
                case "openat":            huarique.OpenAt = value?.ToString(); break;
                case "closeat":           huarique.CloseAt = value?.ToString(); break;
                case "deliveryavailable": huarique.DeliveryAvailable = Convert.ToBoolean(value); break;
                case "takeawayavailable": huarique.TakeawayAvailable = Convert.ToBoolean(value); break;
                case "dineinavailable":   huarique.DineInAvailable = Convert.ToBoolean(value); break;            }
        }

        context.Huariques.Update(huarique);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Huarique huarique, CancellationToken ct = default)
    {
        context.Huariques.Remove(huarique);
        return Task.CompletedTask;
    }
    public async Task<(byte[] Data, string ContentType)?> GetImageAsync(int id, CancellationToken ct = default)
    {
        var row = await context.Huariques
            .Where(h => h.Id == id && h.ImageData != null)
            .Select(h => new { h.ImageData, h.ImageContentType })
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        if (row?.ImageData is null)
            return null;

        return (row.ImageData, row.ImageContentType ?? "image/jpeg");
    }
}
