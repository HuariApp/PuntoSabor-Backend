// PuntoSabor-Backend.Tests/AuthServicesUnitTests.cs
//
// Pruebas unitarias para Auth/Infrastructure/Services/TokenService.cs, que implementa
// el contrato ITokenService declarado en Auth/Application/Services. Es la unica logica
// de negocio real en esa capa que no depende de base de datos ni HTTP: construye y firma
// un JWT a partir de un User y la clave configurada en IConfiguration. IConfiguration se
// construye aqui con ConfigurationBuilder + AddInMemoryCollection (objeto real, no un
// mock), por lo que no se necesita Moq.
//
// Fuera de alcance (no testeable sin mocks o sin infraestructura externa):
// - AuthController (Login, UpdateProfile, ResetPassword, etc.) depende de IUserRepository
//   e IUnitOfWork (EF Core / base de datos), que no tienen implementacion in-memory en
//   este proyecto y Moq no esta instalado.
// - La VALIDACION de tokens (firma invalida, token expirado, token mal formado) no tiene
//   logica propia en el codigo: la hace el middleware JwtBearer de ASP.NET Core,
//   configurado en Program.cs vía AddJwtBearer, que solo se puede probar con un
//   WebApplicationFactory (prueba de integracion), no con un test unitario puro.
//
// Framework: xUnit. Patron: AAA (Preparar / Ejecutar / Verificar).

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Xunit;
using PuntoSabor_Backend.Auth.Domain.Model;
using PuntoSabor_Backend.Auth.Infrastructure.Services;

namespace PuntoSabor_Backend.Tests
{
    public class TokenServiceTests
    {
        private const string SecretDePrueba = "una-clave-secreta-de-prueba-con-al-menos-32-bytes!!";

        private static IConfiguration CrearConfiguracion(string? secret)
        {
            var datos = secret is null
                ? new System.Collections.Generic.Dictionary<string, string?>()
                : new System.Collections.Generic.Dictionary<string, string?> { ["Jwt:Secret"] = secret };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(datos)
                .Build();
        }

        private static User CrearUsuario(UserRole role = UserRole.Consumer) => new()
        {
            Id = 7,
            Name = "Carlos",
            Email = "carlos@test.com",
            PasswordHash = "hash-irrelevante",
            Role = role
        };

        [Fact]
        public void GenerateToken_ConSecretConfigurado_IncluyeLosClaimsDelUsuarioYExpiraEn7Dias()
        {
            var service = new TokenService(CrearConfiguracion(SecretDePrueba));
            var usuario = CrearUsuario(UserRole.Owner);

            var antes = DateTime.UtcNow;
            var token = service.GenerateToken(usuario);
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            Assert.Equal("7", jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            Assert.Equal("Carlos", jwt.Claims.First(c => c.Type == ClaimTypes.Name).Value);
            Assert.Equal("carlos@test.com", jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value);
            Assert.Equal("owner", jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
            Assert.True(jwt.ValidTo > antes.AddDays(6).AddHours(23));
            Assert.True(jwt.ValidTo < antes.AddDays(7).AddHours(1));
        }

        [Fact]
        public void GenerateToken_ConSecretConfigurado_FirmaElTokenConLaMismaClave()
        {
            var service = new TokenService(CrearConfiguracion(SecretDePrueba));
            var usuario = CrearUsuario();

            var token = service.GenerateToken(usuario);

            var parametros = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretDePrueba))
            };

            // No debe lanzar: la firma es valida con la misma clave usada para generarlo.
            new JwtSecurityTokenHandler().ValidateToken(token, parametros, out _);
        }

        [Fact]
        public void GenerateToken_ConClaveDeFirmaDistinta_LaValidacionFalla()
        {
            // Caso de error: un token firmado con un secreto no puede validarse con otro.
            var service = new TokenService(CrearConfiguracion(SecretDePrueba));
            var usuario = CrearUsuario();
            var token = service.GenerateToken(usuario);

            var parametrosConClaveIncorrecta = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes("otra-clave-secreta-totalmente-distinta-32bytes!"))
            };

            Assert.Throws<SecurityTokenSignatureKeyNotFoundException>(() =>
                new JwtSecurityTokenHandler().ValidateToken(token, parametrosConClaveIncorrecta, out _));
        }

        [Fact]
        public void GenerateToken_SinJwtSecretConfigurado_LanzaInvalidOperationException()
        {
            var service = new TokenService(CrearConfiguracion(secret: null));
            var usuario = CrearUsuario();

            Assert.Throws<InvalidOperationException>(() => service.GenerateToken(usuario));
        }
    }
}
