// PuntoSabor-Backend.Tests/AssemblerUnitTests.cs
//
// Pruebas unitarias para los Assemblers de Presentation/Transform.
// Framework: xUnit. Patron: AAA (Preparar / Ejecutar / Verificar).
//
// Cubre: CreateHuariqueEntityFromResourceAssembler, CreatePromoEntityFromResourceAssembler,
// CreateSubscriptionEntityFromResourceAssembler y CreateUserEntityFromResourceAssembler.
// Cada Assembler tiene un caso feliz y un caso borde (campos opcionales nulos o valor
// inesperado). El caso borde de CreateUserEntityFromResourceAssembler documenta el
// comportamiento actual (no corregido) ante un valor de Role con espacios extra:
// ver bug reportado en CreateUserEntityFromResourceAssembler.cs linea 18.

using System;
using Xunit;
using PuntoSabor_Backend.Auth.Domain.Model;
using PuntoSabor_Backend.Discovery.Domain.Model;
using PuntoSabor_Backend.Memberships.Domain.Model;
using PuntoSabor_Backend.Promotions.Domain.Model;
using PuntoSabor_Backend.Presentation.Resources;
using PuntoSabor_Backend.Presentation.Transform;

namespace PuntoSabor_Backend.Tests
{
    public class CreateHuariqueEntityFromResourceAssemblerTests
    {
        [Fact]
        public void ToEntityFromResource_ConTodosLosCamposCompletos_CopiaYRecortaValores()
        {
            var resource = new CreateHuariqueResource(
                Name: " Ceviche Bar ",
                Category: " Marina ",
                CategoryId: 3,
                Price: 25.50m,
                District: " Miraflores ",
                Latitude: -12.1,
                Longitude: -77.0,
                OwnerId: 7,
                Address: " Av. Larco 123 ",
                Phone: " 999888777 ",
                Description: " Rico ceviche ",
                ImageUrl: " http://img.test/1.png ",
                OpenAt: " 09:00 ",
                CloseAt: " 22:00 ",
                DeliveryAvailable: true,
                TakeawayAvailable: true,
                DineInAvailable: false);

            var entity = CreateHuariqueEntityFromResourceAssembler.ToEntityFromResource(resource);

            Assert.Equal("Ceviche Bar", entity.Name);
            Assert.Equal("Marina", entity.Category);
            Assert.Equal(3, entity.CategoryId);
            Assert.Equal(25.50m, entity.Price);
            Assert.Equal("Miraflores", entity.District);
            Assert.Equal(0, entity.Rating);
            Assert.False(entity.Near);
            Assert.Equal(-12.1, entity.Latitude);
            Assert.Equal(-77.0, entity.Longitude);
            Assert.Equal(7, entity.OwnerId);
            Assert.Equal("Av. Larco 123", entity.Address);
            Assert.Equal("999888777", entity.Phone);
            Assert.Equal("Rico ceviche", entity.Description);
            Assert.Equal("http://img.test/1.png", entity.ImageUrl);
            Assert.Equal("09:00", entity.OpenAt);
            Assert.Equal("22:00", entity.CloseAt);
            Assert.True(entity.DeliveryAvailable);
            Assert.True(entity.TakeawayAvailable);
            Assert.False(entity.DineInAvailable);
        }

        [Fact]
        public void ToEntityFromResource_ConCamposOpcionalesNulos_NoLanzaYMantieneNulos()
        {
            var resource = new CreateHuariqueResource(
                Name: "Pollo a la Brasa",
                Category: "Parrilla",
                CategoryId: 1,
                Price: 18m,
                District: "San Isidro");

            var entity = CreateHuariqueEntityFromResourceAssembler.ToEntityFromResource(resource);

            Assert.Null(entity.Latitude);
            Assert.Null(entity.Longitude);
            Assert.Null(entity.OwnerId);
            Assert.Null(entity.Address);
            Assert.Null(entity.Phone);
            Assert.Null(entity.Description);
            Assert.Null(entity.ImageUrl);
            Assert.Null(entity.OpenAt);
            Assert.Null(entity.CloseAt);
            Assert.False(entity.DeliveryAvailable);
            Assert.False(entity.TakeawayAvailable);
            Assert.True(entity.DineInAvailable);
        }
    }

    public class CreatePromoEntityFromResourceAssemblerTests
    {
        [Fact]
        public void ToEntityFromResource_ConTipoEnMayusculasYConEspacios_NormalizaATipoEnMinusculas()
        {
            var resource = new CreatePromoResource(
                Title: " 2x1 en ceviche ",
                Note: " Solo fines de semana ",
                HuariqueId: 5,
                Type: " 2X1 ",
                Discount: 0,
                Code: " PROMO2X1 ",
                StartDate: new DateTime(2026, 1, 1),
                EndDate: new DateTime(2026, 1, 31),
                MaxUses: 100);

            var entity = CreatePromoEntityFromResourceAssembler.ToEntityFromResource(resource);

            Assert.Equal("2x1 en ceviche", entity.Title);
            Assert.Equal("Solo fines de semana", entity.Note);
            Assert.Equal(5, entity.HuariqueId);
            Assert.Equal("2x1", entity.Type);
            Assert.Equal("PROMO2X1", entity.Code);
            Assert.Equal(new DateTime(2026, 1, 1), entity.StartDate);
            Assert.Equal(new DateTime(2026, 1, 31), entity.EndDate);
            Assert.Equal(100, entity.MaxUses);
        }

        [Fact]
        public void ToEntityFromResource_ConCamposOpcionalesNulos_CurrentUsesQuedaEnCero()
        {
            var resource = new CreatePromoResource(
                Title: "Descuento general",
                Note: "Aplica todos los dias");

            var entity = CreatePromoEntityFromResourceAssembler.ToEntityFromResource(resource);

            Assert.Null(entity.Code);
            Assert.Null(entity.StartDate);
            Assert.Null(entity.EndDate);
            Assert.Null(entity.MaxUses);
            Assert.Null(entity.ImageUrl);
            Assert.Null(entity.HuariqueId);
            Assert.Equal(0, entity.CurrentUses);
            Assert.Equal("otro", entity.Type);
        }
    }

    public class CreateSubscriptionEntityFromResourceAssemblerTests
    {
        [Fact]
        public void ToEntityFromResource_ConPlanIdEnMayusculasYConEspacios_NormalizaYActivaLaSuscripcion()
        {
            var resource = new CreateSubscriptionResource(
                UserId: 42,
                PlanId: " PREMIUM ",
                EndDate: new DateTime(2027, 1, 1));

            var antes = DateTime.UtcNow;
            var entity = CreateSubscriptionEntityFromResourceAssembler.ToEntityFromResource(resource);
            var despues = DateTime.UtcNow;

            Assert.Equal(42, entity.UserId);
            Assert.Equal("premium", entity.PlanId);
            Assert.Equal("active", entity.Status);
            Assert.Equal(new DateTime(2027, 1, 1), entity.EndDate);
            Assert.InRange(entity.StartDate, antes, despues);
        }

        [Fact]
        public void ToEntityFromResource_SinFechaDeFin_LaEntidadQuedaConEndDateNulo()
        {
            var resource = new CreateSubscriptionResource(
                UserId: 1,
                PlanId: "basico");

            var entity = CreateSubscriptionEntityFromResourceAssembler.ToEntityFromResource(resource);

            Assert.Null(entity.EndDate);
            Assert.True(entity.IsActive);
        }
    }

    public class CreateUserEntityFromResourceAssemblerTests
    {
        [Fact]
        public void ToEntityFromResource_ConRoleOwnerExacto_AsignaRolOwnerYHasheaPassword()
        {
            var resource = new CreateUserResource(
                Name: " Carlos ",
                Email: " carlos@test.com ",
                Password: "Secreta123",
                Role: "owner");

            var entity = CreateUserEntityFromResourceAssembler.ToEntityFromResource(resource);

            Assert.Equal("Carlos", entity.Name);
            Assert.Equal("carlos@test.com", entity.Email);
            Assert.Equal(UserRole.Owner, entity.Role);
            Assert.NotEqual("Secreta123", entity.PasswordHash);
            Assert.True(BCrypt.Net.BCrypt.Verify("Secreta123", entity.PasswordHash));
        }

        [Fact]
        public void ToEntityFromResource_ConRoleOwnerConEspaciosExtra_AsignaRolOwner()
        {
            // CreateUserEntityFromResourceAssembler.cs linea 18 ahora recorta espacios
            // antes de comparar (resource.Role?.Trim().ToLowerInvariant() == "owner"),
            // por lo que un Role valido con espacios ya no se degrada a Consumer.
            var resource = new CreateUserResource(
                Name: "Ana",
                Email: "ana@test.com",
                Password: "Secreta123",
                Role: " owner ");

            var entity = CreateUserEntityFromResourceAssembler.ToEntityFromResource(resource);

            Assert.Equal(UserRole.Owner, entity.Role);
        }

        [Fact]
        public void ToEntityFromResource_SinRoleEspecificado_UsaConsumerPorDefecto()
        {
            var resource = new CreateUserResource(
                Name: "Luis",
                Email: "luis@test.com",
                Password: "Secreta123");

            var entity = CreateUserEntityFromResourceAssembler.ToEntityFromResource(resource);

            Assert.Equal(UserRole.Consumer, entity.Role);
        }
    }

    public class CreateCategoryEntityFromResourceAssemblerTests
    {
        [Fact]
        public void ToEntityFromResource_ConEspaciosAlInicioYAlFinal_LosRecorta()
        {
            var resource = new CreateCategoryResource(" Comida Rapida ");

            var entity = CreateCategoryEntityFromResourceAssembler.ToEntityFromResource(resource);

            Assert.Equal("Comida Rapida", entity.Name);
        }

        [Fact]
        public void ToEntityFromResource_ConEspaciosDoblesEnMedio_NoLosColapsa()
        {
            // Caso borde: Trim() solo recorta los extremos, no normaliza espacios
            // internos repetidos. Documenta el comportamiento real del Assembler.
            var resource = new CreateCategoryResource(" Comida  Rapida ");

            var entity = CreateCategoryEntityFromResourceAssembler.ToEntityFromResource(resource);

            Assert.Equal("Comida  Rapida", entity.Name);
        }
    }

    public class HuariqueResourceFromEntityAssemblerTests
    {
        [Fact]
        public void ToResourceFromEntity_ConEntidadCompleta_CopiaTodosLosCamposEnOrden()
        {
            var entity = new Huarique
            {
                Id = 10,
                Name = "Ceviche Bar",
                Category = "Marina",
                CategoryId = 3,
                Price = 25.50m,
                Rating = 4.5,
                District = "Miraflores",
                Near = true,
                Latitude = -12.1,
                Longitude = -77.0,
                OwnerId = 7,
                Address = "Av. Larco 123",
                Phone = "999888777",
                Description = "Rico ceviche",
                ImageUrl = "http://img.test/1.png",
                OpenAt = "09:00",
                CloseAt = "22:00",
                DeliveryAvailable = true,
                TakeawayAvailable = false,
                DineInAvailable = true,
                CreatedAt = new DateTime(2026, 1, 1),
                UpdatedAt = new DateTime(2026, 2, 1)
            };

            var resource = HuariqueResourceFromEntityAssembler.ToResourceFromEntity(entity);

            Assert.Equal(10, resource.Id);
            Assert.Equal("Ceviche Bar", resource.Name);
            Assert.Equal("Marina", resource.Category);
            Assert.Equal(3, resource.CategoryId);
            Assert.Equal(25.50m, resource.Price);
            Assert.Equal(4.5, resource.Rating);
            Assert.Equal("Miraflores", resource.District);
            Assert.True(resource.Near);
            Assert.Equal(-12.1, resource.Latitude);
            Assert.Equal(-77.0, resource.Longitude);
            Assert.Equal(7, resource.OwnerId);
            Assert.Equal("Av. Larco 123", resource.Address);
            Assert.Equal("999888777", resource.Phone);
            Assert.Equal("Rico ceviche", resource.Description);
            Assert.Equal("http://img.test/1.png", resource.ImageUrl);
            Assert.Equal("09:00", resource.OpenAt);
            Assert.Equal("22:00", resource.CloseAt);
            Assert.True(resource.DeliveryAvailable);
            Assert.False(resource.TakeawayAvailable);
            Assert.True(resource.DineInAvailable);
            Assert.Equal(new DateTime(2026, 1, 1), resource.CreatedAt);
            Assert.Equal(new DateTime(2026, 2, 1), resource.UpdatedAt);
        }

        [Fact]
        public void ToResourceFromEntity_ConCamposOpcionalesNulos_PropagaLosNulosSinLanzar()
        {
            var entity = new Huarique
            {
                Id = 11,
                Name = "Pollo a la Brasa",
                Category = "Parrilla",
                CategoryId = 1,
                Price = 18m,
                District = "San Isidro",
                UpdatedAt = null
            };

            var resource = HuariqueResourceFromEntityAssembler.ToResourceFromEntity(entity);

            Assert.Null(resource.Latitude);
            Assert.Null(resource.Longitude);
            Assert.Null(resource.OwnerId);
            Assert.Null(resource.Address);
            Assert.Null(resource.Phone);
            Assert.Null(resource.Description);
            Assert.Null(resource.ImageUrl);
            Assert.Null(resource.OpenAt);
            Assert.Null(resource.CloseAt);
            Assert.Null(resource.UpdatedAt);
        }
    }

    public class PromoResourceFromEntityAssemblerTests
    {
        [Fact]
        public void ToResourceFromEntity_ConPromoDentroDeRango_PropagaIsActiveEnTrue()
        {
            var entity = new Promo
            {
                Id = 1,
                Title = "2x1 en ceviche",
                Note = "Solo fines de semana",
                Type = "2x1",
                Discount = 0,
                Code = "PROMO1",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(5),
                MaxUses = 10,
                CurrentUses = 2,
                HuariqueId = 4,
                ImageUrl = "http://img.test/promo.png"
            };

            var resource = PromoResourceFromEntityAssembler.ToResourceFromEntity(entity);

            Assert.Equal(1, resource.Id);
            Assert.Equal("2x1 en ceviche", resource.Title);
            Assert.Equal("2x1", resource.Type);
            Assert.Equal(10, resource.MaxUses);
            Assert.Equal(2, resource.CurrentUses);
            Assert.Equal(4, resource.HuariqueId);
            Assert.True(resource.IsActive);
        }

        [Fact]
        public void ToResourceFromEntity_ConMaxUsesNuloYSinFechas_PropagaIsActiveEnTrueYCamposNulos()
        {
            // Caso borde: MaxUses nulo hace que Promo.IsActive ignore el limite de usos
            // (ver Promo.cs), el Assembler debe propagar ese calculo tal cual, no
            // recalcularlo por su cuenta.
            var entity = new Promo
            {
                Id = 2,
                Title = "Descuento general",
                Note = "Aplica todos los dias",
                Type = "otro",
                Discount = 10,
                MaxUses = null,
                CurrentUses = 999
            };

            var resource = PromoResourceFromEntityAssembler.ToResourceFromEntity(entity);

            Assert.Null(resource.Code);
            Assert.Null(resource.StartDate);
            Assert.Null(resource.EndDate);
            Assert.Null(resource.MaxUses);
            Assert.Null(resource.HuariqueId);
            Assert.Null(resource.ImageUrl);
            Assert.True(resource.IsActive);
        }
    }

    public class SubscriptionResourceFromEntityAssemblerTests
    {
        [Fact]
        public void ToResourceFromEntity_ConPlanCargado_CopiaNombreYPrecioDelPlan()
        {
            var entity = new Subscription
            {
                Id = 5,
                UserId = 42,
                PlanId = "premium",
                Plan = new Plan { Id = "premium", Name = "Premium", Price = 29.90m },
                Status = "active",
                StartDate = DateTime.UtcNow.AddMonths(-1),
                EndDate = null
            };

            var resource = SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(entity);

            Assert.Equal("premium", resource.PlanId);
            Assert.Equal("Premium", resource.PlanName);
            Assert.Equal(29.90m, resource.PlanPrice);
            Assert.True(resource.IsActive);
        }

        [Fact]
        public void ToResourceFromEntity_ConPlanNoCargado_PlanNameYPlanPriceQuedanNulos()
        {
            // Caso borde que expone un riesgo real: si el repositorio no hace
            // Include(s => s.Plan), la navegacion queda nula y el Assembler
            // devuelve PlanName/PlanPrice en null aunque exista un PlanId valido.
            var entity = new Subscription
            {
                Id = 6,
                UserId = 42,
                PlanId = "premium",
                Plan = null,
                Status = "active",
                StartDate = DateTime.UtcNow.AddMonths(-1)
            };

            var resource = SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(entity);

            Assert.Equal("premium", resource.PlanId);
            Assert.Null(resource.PlanName);
            Assert.Null(resource.PlanPrice);
        }
    }

    public class UserResourceFromEntityAssemblerTests
    {
        [Fact]
        public void ToResourceFromEntity_ConRoleOwner_DevuelveRoleEnMinusculas()
        {
            var entity = new User
            {
                Id = 1,
                Name = "Carlos",
                Email = "carlos@test.com",
                PasswordHash = "hash",
                Role = UserRole.Owner,
                UpdatedAt = new DateTime(2026, 2, 1)
            };

            var resource = UserResourceFromEntityAssembler.ToResourceFromEntity(entity);

            Assert.Equal("owner", resource.Role);
            Assert.Equal(new DateTime(2026, 2, 1), resource.UpdatedAt);
        }

        [Fact]
        public void ToResourceFromEntity_ConRoleConsumerYSinActualizar_DevuelveRoleConsumerYUpdatedAtNulo()
        {
            var entity = new User
            {
                Id = 2,
                Name = "Ana",
                Email = "ana@test.com",
                PasswordHash = "hash",
                Role = UserRole.Consumer,
                UpdatedAt = null
            };

            var resource = UserResourceFromEntityAssembler.ToResourceFromEntity(entity);

            Assert.Equal("consumer", resource.Role);
            Assert.Null(resource.UpdatedAt);
        }
    }
}
