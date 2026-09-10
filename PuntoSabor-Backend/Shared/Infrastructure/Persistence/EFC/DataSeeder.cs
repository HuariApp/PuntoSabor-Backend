using System.Linq;
using BCrypt.Net;
using PuntoSabor_Backend.Auth.Domain.Model;
using PuntoSabor_Backend.Shared.Infrastructure.Persistence.EFC;
using PuntoSabor_Backend.Discovery.Domain.Model;
using PuntoSabor_Backend.Memberships.Domain.Model;
using PuntoSabor_Backend.Promotions.Domain.Model;
using PuntoSabor_Backend.Reviews.Domain.Model;
using Subscription = PuntoSabor_Backend.Memberships.Domain.Model.Subscription;

namespace PuntoSabor_Backend.Shared.Infrastructure.Persistence.EFC;

/**
 * <summary>
 * Pobla la base de datos con datos iniciales para prueba y demo.
 * </summary>
 */

public static class DataSeeder
{
    public static void Seed(AppDbContext context)
    {
        // CATEGORIES
        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                new Category { Id = 1, Name = "Pollo" },
                new Category { Id = 2, Name = "Marina" },
                new Category { Id = 3, Name = "Criolla" },
                new Category { Id = 4, Name = "Chifa" },
                new Category { Id = 5, Name = "Postres" },
                new Category { Id = 6, Name = "Menú" },
                new Category { Id = 7, Name = "Café" },
                new Category { Id = 8, Name = "Parrillas" }
            );
        }

        // HUARIQUES
if (!context.Huariques.Any())
{
    context.Huariques.AddRange(
        new Huarique {
            Id = 1,
            Name = "El Brasero",
            Latitude = -12.1450, Longitude = -76.9900,
            CategoryId = 1,
            Category = "Pollo",
            Price = 22,
            Rating = 4.6,
            District = "Surco",
            Near = true,
            OwnerId = 2
        },

        new Huarique {
            Id = 2,
            Name = "Rincón Marino",
            Latitude = -12.1750, Longitude = -77.0150,
            CategoryId = 2,
            Category = "Marina",
            Price = 28,
            Rating = 4.8,
            District = "Chorrillos",
            Near = false,
            OwnerId = 2
        },

        new Huarique {
            Id = 3,
            Name = "Doña Peta Criolla",
            Latitude = -12.1490, Longitude = -77.0210,
            CategoryId = 3,
            Category = "Criolla",
            Price = 25,
            Rating = 4.5,
            District = "Barranco",
            Near = true
        },

        new Huarique {
            Id = 4,
            Name = "Chifa San Joy Lao",
            Latitude = -12.1210, Longitude = -77.0290,
            CategoryId = 4,
            Category = "Chifa",
            Price = 21,
            Rating = 4.4,
            District = "Miraflores",
            Near = false
        },

        new Huarique {
            Id = 5,
            Name = "La Dulcería",
            Latitude = -12.0970, Longitude = -77.0360,
            CategoryId = 5,
            Category = "Postres",
            Price = 15,
            Rating = 4.9,
            District = "San Isidro",
            Near = false
        },

        new Huarique {
            Id = 6,
            Name = "La Esquinita del Menú",
            Latitude = -12.1080, Longitude = -76.9990,
            CategoryId = 6,
            Category = "Menú",
            Price = 12,
            Rating = 4.2,
            District = "San Borja",
            Near = true
        },

        new Huarique {
            Id = 7,
            Name = "Café Aroma & Sabor",
            Latitude = -12.1180, Longitude = -77.0310,
            CategoryId = 7,
            Category = "Café",
            Price = 10,
            Rating = 4.7,
            District = "Miraflores",
            Near = false
        },

        new Huarique {
            Id = 8,
            Name = "Pollos Don Tito",
            Latitude = -12.0790, Longitude = -76.9440,
            CategoryId = 1,
            Category = "Pollo",
            Price = 24,
            Rating = 4.7,
            District = "La Molina",
            Near = false
        },

        new Huarique {
            Id = 9,
            Name = "Mar & Tierra",
            Latitude = -12.0770, Longitude = -77.0920,
            CategoryId = 2,
            Category = "Marina",
            Price = 30,
            Rating = 4.3,
            District = "San Miguel",
            Near = true
        },

        new Huarique {
            Id = 10,
            Name = "Café Central",
            Latitude = -12.0460, Longitude = -77.0300,
            CategoryId = 7,
            Category = "Café",
            Price = 11,
            Rating = 4.5,
            District = "Centro de Lima",
            Near = false
        },

        new Huarique {
            Id = 11,
            Name = "Parrilladas Don Mario",
            Latitude = -12.1120, Longitude = -77.0150,
            CategoryId = 8,
            Category = "Parrillas",
            Price = 35,
            Rating = 4.8,
            District = "Surquillo",
            Near = true
        },

        new Huarique {
            Id = 12,
            Name = "Brasa y Carbón",
            Latitude = -12.0860, Longitude = -77.0360,
            CategoryId = 8,
            Category = "Parrillas",
            Price = 38,
            Rating = 4.6,
            District = "Lince",
            Near = false
        },

        new Huarique {
            Id = 13,
            Name = "Fuego Criollo",
            Latitude = -12.1590, Longitude = -76.9700,
            CategoryId = 8,
            Category = "Parrillas",
            Price = 36,
            Rating = 4.7,
            District = "San Juan de Miraflores",
            Near = true
        },

        new Huarique {
            Id = 14,
            Name = "La Parrilla del Norte",
            Latitude = -11.9580, Longitude = -77.0700,
            CategoryId = 8,
            Category = "Parrillas",
            Price = 33,
            Rating = 4.5,
            District = "Los Olivos",
            Near = false
        },

        new Huarique {
            Id = 15,
            Name = "Punto Grill",
            Latitude = -12.0990, Longitude = -77.0340,
            CategoryId = 8,
            Category = "Parrillas",
            Price = 40,
            Rating = 4.9,
            District = "San Isidro",
            Near = false
        },

        new Huarique {
            Id = 16,
            Name = "La Picantería Peruana",
            Latitude = -12.1140, Longitude = -77.0130,
            CategoryId = 3,
            Category = "Criolla",
            Price = 27,
            Rating = 4.7,
            District = "Surquillo",
            Near = true
        },

        new Huarique {
            Id = 17,
            Name = "La Casa del Postre",
            Latitude = -12.1100, Longitude = -77.0170,
            CategoryId = 5,
            Category = "Postres",
            Price = 13,
            Rating = 4.7,
            District = "Surquillo",
            Near = true
        },

        new Huarique {
            Id = 18,
            Name = "Chifa Ping Chung Long",
            Latitude = -12.0880, Longitude = -77.0340,
            CategoryId = 4,
            Category = "Chifa",
            Price = 20,
            Rating = 4.2,
            District = "Lince",
            Near = false
        },

        new Huarique {
            Id = 19,
            Name = "El Sabor Norteño",
            Latitude = -11.9600, Longitude = -77.0680,
            CategoryId = 3,
            Category = "Criolla",
            Price = 23,
            Rating = 4.2,
            District = "Los Olivos",
            Near = false
        },

        new Huarique {
            Id = 20,
            Name = "La Ola Marina",
            Latitude = -12.0920, Longitude = -77.0720,
            CategoryId = 2,
            Category = "Marina",
            Price = 29,
            Rating = 4.4,
            District = "Magdalena",
            Near = true
        },

        new Huarique {
            Id = 21,
            Name = "Menu Don Lucho",
            Latitude = -12.0420, Longitude = -76.9990,
            CategoryId = 6,
            Category = "Menú",
            Price = 12,
            Rating = 4.2,
            District = "El Agustino",
            Near = true
        }
    );
}


        // PROMOS
        if (!context.Promos.Any())
        {
            var now = DateTime.UtcNow;
            context.Promos.AddRange(
                new Promo { Id = 1, Title = "2x1 Pollo Hoy",       Note = "Locales seleccionados",      Type = "2x1",       Discount = 50, HuariqueId = 1, EndDate = now.AddDays(7)  },
                new Promo { Id = 2, Title = "Menú Marino S/20",    Note = "Lun–Vie 12:00–16:00",        Type = "menu",      Discount = 0,  HuariqueId = 2, EndDate = now.AddDays(14) },
                new Promo { Id = 3, Title = "Café + Brownie",      Note = "Solo en Aroma & Sabor",      Type = "otro",      Discount = 0,  HuariqueId = 7                             },
                new Promo { Id = 4, Title = "Parrillada Familiar", Note = "15% en fines de semana",     Type = "descuento", Discount = 15, HuariqueId = 11, EndDate = now.AddDays(30) },
                new Promo { Id = 5, Title = "Postres 3x2",         Note = "En La Dulcería todo el mes", Type = "otro",      Discount = 33, HuariqueId = 5,  EndDate = now.AddDays(30) },
                new Promo { Id = 6, Title = "Descuento Criollo",   Note = "Platos criollos -10%",       Type = "descuento", Discount = 10, HuariqueId = 3,  EndDate = now.AddDays(7)  }
            );
        }

        // PLANS
        if (!context.Plans.Any())
        {
            context.Plans.AddRange(
                new Plan { Id = "premium",   Name = "Premium",   Price = 35 },
                new Plan { Id = "basic",     Name = "Básico",    Price = 0 },
                new Plan { Id = "exclusive", Name = "Exclusive", Price = 50 }
            );
        }

        // USERS demo — password: demo1234
        // User 2 (SoyElPepe) es propietario demo con 2 huariques asignados
        if (!context.Users.Any())
        {
            var demoHash = BCrypt.Net.BCrypt.HashPassword("demo1234");
            context.Users.AddRange(
                new User { Id = 1, Name = "demo",        Email = "demo@upc.edu.pe",      PasswordHash = demoHash, Role = UserRole.Consumer },
                new User { Id = 2, Name = "SoyElPepe",   Email = "wa@gmail.com",          PasswordHash = demoHash, Role = UserRole.Owner    },
                new User { Id = 3, Name = "Luna C.",     Email = "luna@puntosabor.pe",    PasswordHash = demoHash, Role = UserRole.Consumer },
                new User { Id = 4, Name = "Sebastián",   Email = "sebastian@upc.pe",      PasswordHash = demoHash, Role = UserRole.Consumer }
            );
        }

        // REVIEWS
        if (!context.Reviews.Any())
        {
            context.Reviews.AddRange(
                new Review { Id = 1,  HuariqueId = 1,  UserId = 1, Rating = 5, Comment = "Buenazo y barato.",                                      CreatedAt = DateTime.Parse("2025-11-01T15:00:00Z") },
                new Review { Id = 2,  HuariqueId = 2,  UserId = 2, Rating = 4, Comment = "Rincón Marino excelente, pero demora un poco.",          CreatedAt = DateTime.Parse("2025-11-02T12:10:00Z") },
                new Review { Id = 3,  HuariqueId = 5,  UserId = 3, Rating = 5, Comment = "La Dulcería, los mejores postres del sur.",              CreatedAt = DateTime.Parse("2025-11-02T18:40:00Z") },
                new Review { Id = 4,  HuariqueId = 11, UserId = 2, Rating = 4, Comment = "Fuego Criollo espectacular, carnes suaves.",            CreatedAt = DateTime.Parse("2025-11-03T10:05:00Z") },
                new Review { Id = 5,  HuariqueId = 15, UserId = 4, Rating = 5, Comment = "Punto Grill es otro nivel, todo premium.",              CreatedAt = DateTime.Parse("2025-11-03T21:25:00Z") },
                new Review { Id = 6,  HuariqueId = 1,  UserId = 3, Rating = 3, Comment = "El Brasero normalito, pero buen precio.",               CreatedAt = DateTime.Parse("2025-11-04T09:15:00Z") },
                new Review { Id = 7,  HuariqueId = 3,  UserId = 1, Rating = 4, Comment = "Doña Peta Criolla buen sazón, menú variado.",           CreatedAt = DateTime.Parse("2025-11-04T14:30:00Z") },
                new Review { Id = 8,  HuariqueId = 11, UserId = 2, Rating = 5, Comment = "Don Mario la rompe con su parrillada familiar.",        CreatedAt = DateTime.Parse("2025-11-05T11:50:00Z") },
                new Review { Id = 9,  HuariqueId = 12, UserId = 4, Rating = 4, Comment = "Brasa y Carbón deliciosa, atención rápida.",            CreatedAt = DateTime.Parse("2025-11-05T19:00:00Z") },
                new Review { Id = 10, HuariqueId = 10, UserId = 3, Rating = 5, Comment = "Café Central con el mejor espresso.",                   CreatedAt = DateTime.Parse("2025-11-06T08:45:00Z") }
            );
        }

        // SUBSCRIPTIONS
        if (!context.Subscriptions.Any())
        {
            context.Subscriptions.AddRange(
                // User 2 (SoyElPepe, owner) — Premium plan activo
                new Subscription { Id = 1, UserId = 2, PlanId = "premium",   StartDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), Status = "active"    },
                // User 1 (demo) — Básico activo
                new Subscription { Id = 2, UserId = 1, PlanId = "basic",     StartDate = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc), Status = "active"    },
                // User 3 (Luna) — Premium cancelado (histórico)
                new Subscription { Id = 3, UserId = 3, PlanId = "premium",   StartDate = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                                   EndDate  = new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Utc), Status = "cancelled" }
            );
        }

        context.SaveChanges();
    }
}
