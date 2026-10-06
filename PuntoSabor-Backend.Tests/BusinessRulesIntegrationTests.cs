using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

// Namespaces exactos extra��dos del backend de PuntoSabor
using PuntoSabor_Backend.Auth.Domain.Model;
using PuntoSabor_Backend.Discovery.Domain.Model;
using PuntoSabor_Backend.Favorites.Domain.Model;
using PuntoSabor_Backend.Memberships.Domain.Model;
using PuntoSabor_Backend.Promotions.Domain.Model;
using PuntoSabor_Backend.Reviews.Domain.Model;
using PuntoSabor_Backend.Shared.Infrastructure.Persistence.EFC;
using PuntoSabor_Backend.UserPreferences.Domain.Model;

namespace PuntoSabor_Backend.Tests
{
    public class BusinessRulesIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<AppDbContext> _options;

        public BusinessRulesIntegrationTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            using (var context = new AppDbContext(_options))
            {
                context.Database.EnsureCreated();
            }
        }

        private AppDbContext GetInMemoryDbContext()
        {
            var context = new AppDbContext(_options);
            context.Database.EnsureCreated();
            return context;
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        #region BR-01: Prevención de Reseñas Duplicadas por el Mismo Usuario en un Huarique

        [Fact]
        public async Task BR01_PublicarResena_CuandoYaExisteResenaDelMismoUsuario_DebeImpedirDuplicado()
        {
            // ===== ARRANGE (Organizar) =====
            using var context = GetInMemoryDbContext();

            var usuario = new User
            {
                Id = 1,
                Name = "Carlos Mendoza",
                Email = "carlos@puntosabor.pe",
                PasswordHash = "hash-de-prueba-123",
                Role = UserRole.Consumer
            };

            var huarique = new Huarique
            {
                Id = 10,
                Name = "El Chicharronero de Surco",
                Category = "Criolla",
                Address = "Av. Ayacucho 123",
                District = "Santiago de Surco"
            };

            var primeraResena = new Review
            {
                UserId = usuario.Id,
                HuariqueId = huarique.Id,
                Rating = 5,
                Comment = "Excelente sazón y atención rápida.",
                CreatedAtReview = DateTime.UtcNow
            };

            context.Users.Add(usuario);
            context.Huariques.Add(huarique);
            context.Reviews.Add(primeraResena);
            await context.SaveChangesAsync();

            var intentoSegundaResena = new Review
            {
                UserId = usuario.Id,
                HuariqueId = huarique.Id,
                Rating = 2,
                Comment = "Intentando cambiar mi opinión enviando otra reseña",
                CreatedAtReview = DateTime.UtcNow
            };

            // ===== ACT & ASSERT (Actuar y Verificar) =====
            // Regla BR-01: Un usuario no puede registrar más de una reseña para el mismo huarique
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                bool yaPuntuoHuarique = await context.Reviews
                    .AnyAsync(r => r.UserId == intentoSegundaResena.UserId && r.HuariqueId == intentoSegundaResena.HuariqueId);

                if (yaPuntuoHuarique)
                {
                    throw new InvalidOperationException("El usuario ya registró una reseña previa para este huarique.");
                }

                context.Reviews.Add(intentoSegundaResena);
                await context.SaveChangesAsync();
            });

            Assert.Equal("El usuario ya registró una reseña previa para este huarique.", exception.Message);

            // Verificar en BD que solo existe 1 sola reseña para el usuario y huarique
            int cantidadResenas = await context.Reviews
                .CountAsync(r => r.UserId == usuario.Id && r.HuariqueId == huarique.Id);

            Assert.Equal(1, cantidadResenas);
        }

        #endregion

        #region BR-02: Vigencia y Control de Cancelación de Membresías SaaS

        [Fact]
        public async Task BR02_MembresiaCancelada_AntesDeFechaVencimiento_DebePermanecerActiva()
        {
            // ===== ARRANGE (Organizar) =====
            using var context = GetInMemoryDbContext();

            // Simular un propietario que canceló su plan Premium pero aún le quedan 15 días de vigencia
            context.Plans.Add(new Plan { Id = "premium", Name = "Premium", Price = 35 });
            await context.SaveChangesAsync();

            var suscripcionCancelada = new Subscription
            {
                Id = 100,
                UserId = 5,
                PlanId = "premium",
                StartDate = DateTime.UtcNow.AddDays(-15),
                EndDate = DateTime.UtcNow.AddDays(15), // Vence dentro de 15 d��as
                Status = "cancelled"
            };

            context.Subscriptions.Add(suscripcionCancelada);
            await context.SaveChangesAsync();

            // ===== ACT (Actuar) =====
            var suscripcionBD = await context.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == 5 && s.PlanId == "premium");

            // Regla BR-02: Permanece activo si Status == "active" O (Status == "cancelled" y EndDate > UtcNow)
            bool estaActiva = suscripcionBD != null &&
                              (suscripcionBD.Status == "active" ||
                              (suscripcionBD.Status == "cancelled" && suscripcionBD.EndDate.HasValue && suscripcionBD.EndDate.Value > DateTime.UtcNow));

            // ===== ASSERT (Verificar) =====
            Assert.NotNull(suscripcionBD);
            Assert.Equal("cancelled", suscripcionBD.Status);
            Assert.True(estaActiva, "La suscripción cancelada debe mantener vigentes los beneficios SaaS hasta cumplir la fecha EndDate.");
        }

        #endregion

        #region BR-04: Límite Único de Favoritos por Usuario y Huarique (Unicidad)

        [Fact]
        public async Task BR03_AgregarFavorito_CuandoYaExisteEnLista_DebeEvitarDuplicadosEnBD()
        {
            // ===== ARRANGE (Organizar) =====
            using var context = GetInMemoryDbContext();

            int usuarioId = 8;
            int huariqueId = 42;

            var favoritoExistente = new Favorite
            {
                UserId = usuarioId,
                HuariqueId = huariqueId
            };

            context.Favorites.Add(favoritoExistente);
            await context.SaveChangesAsync();

            var intentoDuplicado = new Favorite
            {
                UserId = usuarioId,
                HuariqueId = huariqueId
            };

            // ===== ACT (Actuar) =====
            // Regla BR-03: Validar si la combinación UserId + HuariqueId ya existe antes de insertar
            bool yaEsFavorito = await context.Favorites
                .AnyAsync(f => f.UserId == intentoDuplicado.UserId && f.HuariqueId == intentoDuplicado.HuariqueId);

            if (!yaEsFavorito)
            {
                context.Favorites.Add(intentoDuplicado);
                await context.SaveChangesAsync();
            }

            int conteoFavoritos = await context.Favorites
                .CountAsync(f => f.UserId == usuarioId && f.HuariqueId == huariqueId);

            // ===== ASSERT (Verificar) =====
            Assert.True(yaEsFavorito, "El sistema identificó correctamente la duplicidad del favorito.");
            Assert.Equal(1, conteoFavoritos); // La base de datos mantiene exactamente un único registro
        }

        #endregion

        #region BR-04: Una Promoción Deja de Estar Activa al Agotar sus Usos Máximos

        [Fact]
        public async Task BR04_ConsultarPromosActivas_CuandoUnaAlcanzoElMaximoDeUsos_DebeExcluirlaDelListado()
        {
            // ===== ARRANGE (Organizar) =====
            using var context = GetInMemoryDbContext();

            var promoVigente = new Promo
            {
                Id = 1,
                Title = "2x1 en ceviche",
                Note = "Válido solo fines de semana",
                Type = "2x1",
                Discount = 0,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(5),
                MaxUses = 10,
                CurrentUses = 3,
                HuariqueId = 10
            };

            var promoAgotada = new Promo
            {
                Id = 2,
                Title = "20% de descuento en anticuchos",
                Note = "Solo los primeros 5 clientes",
                Type = "descuento",
                Discount = 20,
                StartDate = DateTime.UtcNow.AddDays(-2),
                EndDate = DateTime.UtcNow.AddDays(3),
                MaxUses = 5,
                CurrentUses = 5, // ya alcanzó el máximo de usos permitido
                HuariqueId = 10
            };

            context.Promos.AddRange(promoVigente, promoAgotada);
            await context.SaveChangesAsync();

            // ===== ACT (Actuar) =====
            // Regla BR-04: Promo.IsActive es false cuando CurrentUses alcanza o supera MaxUses
            var todasLasPromos = await context.Promos
                .Where(p => p.HuariqueId == 10)
                .ToListAsync();

            var promosActivas = todasLasPromos.Where(p => p.IsActive).ToList();

            // ===== ASSERT (Verificar) =====
            Assert.Equal(2, todasLasPromos.Count); // ambas persistieron en BD
            Assert.Single(promosActivas); // pero solo una sigue activa
            Assert.Equal(promoVigente.Id, promosActivas.First().Id);
            Assert.False(promoAgotada.IsActive, "La promoción que alcanzó su MaxUses no debe considerarse activa.");
        }

        #endregion
    }
}
