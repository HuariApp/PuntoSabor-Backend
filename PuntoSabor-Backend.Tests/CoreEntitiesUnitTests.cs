// PuntoSabor-Backend.Tests/CoreEntitiesUnitTests.cs
//
// 7 pruebas unitarias para las entidades Promo y Subscription.
// Framework: xUnit. Patron: AAA (Preparar / Ejecutar / Verificar).

using System;
using Xunit;
using PuntoSabor_Backend.Promotions.Domain.Model;
using PuntoSabor_Backend.Memberships.Domain.Model;

namespace PuntoSabor_Backend.Tests
{
    public class PromoTests
    {
        private Promo CrearPromoBase()
        {
            return new Promo
            {
                Title = "2x1 en ceviche",
                Note = "Valido solo fines de semana",
                Type = "2x1",
                Discount = 0,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(5),
                MaxUses = 10,
                CurrentUses = 0,
                HuariqueId = 1
            };
        }

        [Fact]
        public void IsActive_CuandoEstaDentroDeRangoYConCuposDisponibles_DevuelveTrue()
        {
            var promo = CrearPromoBase();
            var resultado = promo.IsActive;
            Assert.True(resultado);
        }

        [Fact]
        public void IsActive_CuandoYaExpiro_DevuelveFalse()
        {
            var promo = CrearPromoBase();
            promo.StartDate = DateTime.UtcNow.AddDays(-10);
            promo.EndDate = DateTime.UtcNow.AddDays(-1);
            var resultado = promo.IsActive;
            Assert.False(resultado);
        }

        [Fact]
        public void IsActive_CuandoAunNoComienza_DevuelveFalse()
        {
            var promo = CrearPromoBase();
            promo.StartDate = DateTime.UtcNow.AddDays(3);
            promo.EndDate = DateTime.UtcNow.AddDays(10);
            var resultado = promo.IsActive;
            Assert.False(resultado);
        }

        [Fact]
        public void IsActive_CuandoAlcanzoElMaximoDeUsos_DevuelveFalse()
        {
            var promo = CrearPromoBase();
            promo.MaxUses = 5;
            promo.CurrentUses = 5;
            var resultado = promo.IsActive;
            Assert.False(resultado);
        }

        [Fact]
        public void IsActive_CuandoMaxUsesEsNulo_IgnoraElLimiteDeUsos()
        {
            var promo = CrearPromoBase();
            promo.MaxUses = null;
            promo.CurrentUses = 999;
            var resultado = promo.IsActive;
            Assert.True(resultado);
        }
    }

    public class SubscriptionTests
    {
        private Subscription CrearSubscripcionBase()
        {
            return new Subscription
            {
                UserId = 1,
                PlanId = "premium",
                StartDate = DateTime.UtcNow.AddMonths(-1),
                EndDate = null,
                Status = "active"
            };
        }

        [Fact]
        public void IsActive_CuandoStatusActivoYSinVencimiento_DevuelveTrue()
        {
            var suscripcion = CrearSubscripcionBase();
            var resultado = suscripcion.IsActive;
            Assert.True(resultado);
        }

        [Fact]
        public void IsActive_CuandoStatusEsCancelled_DevuelveFalse()
        {
            var suscripcion = CrearSubscripcionBase();
            suscripcion.Status = "cancelled";
            var resultado = suscripcion.IsActive;
            Assert.False(resultado);
        }
    }
}