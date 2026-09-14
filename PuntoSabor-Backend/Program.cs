using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PuntoSabor_Backend.Auth.Application.Services;
using PuntoSabor_Backend.Auth.Domain.Repositories;
using PuntoSabor_Backend.Auth.Infrastructure.Persistence.EFC.Repositories;
using PuntoSabor_Backend.Auth.Infrastructure.Services;
using PuntoSabor_Backend.Discovery.Domain.Repositories;
using PuntoSabor_Backend.Discovery.Infrastructure.Persistence.EFC.Repositories;
using PuntoSabor_Backend.Favorites.Domain.Repositories;
using PuntoSabor_Backend.Favorites.Infrastructure.Persistence.EFC.Repositories;
using PuntoSabor_Backend.Memberships.Domain.Repositories;
using PuntoSabor_Backend.Memberships.Infrastructure.Persistence.EFC.Repositories;
using SubscriptionRepository = PuntoSabor_Backend.Memberships.Infrastructure.Persistence.EFC.Repositories.SubscriptionRepository;
using PuntoSabor_Backend.Promotions.Domain.Repositories;
using PuntoSabor_Backend.Promotions.Infrastructure.Persistence.EFC.Repositories;
using PuntoSabor_Backend.Reviews.Domain.Repositories;
using PuntoSabor_Backend.Reviews.Infrastructure.Persistence.EFC.Repositories;
using PuntoSabor_Backend.Notifications.Domain.Repositories;
using PuntoSabor_Backend.Notifications.Infrastructure.Persistence.EFC.Repositories;
using PuntoSabor_Backend.Reports.Domain.Repositories;
using PuntoSabor_Backend.Reports.Infrastructure.Persistence.EFC.Repositories;
using PuntoSabor_Backend.UserPreferences.Domain.Repositories;
using PuntoSabor_Backend.UserPreferences.Infrastructure.Persistence.EFC.Repositories;
using PuntoSabor_Backend.Shared.Domain.Repositories;
using PuntoSabor_Backend.Shared.Infrastructure.Persistence.EFC;

var builder = WebApplication.CreateBuilder(args);

// Helper: intenta varias variables de entorno en orden y devuelve la primera que tenga valor.
static string? FirstEnv(params string[] names)
{
    foreach (var name in names)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (!string.IsNullOrWhiteSpace(value)) return value;
    }
    return null;
}

// Acepta tanto las variables custom (DB_HOST, etc.) como las que Railway
// genera automaticamente al agregar un plugin de MySQL (MYSQLHOST, etc.)
// o la MYSQL_URL en formato mysql://user:pass@host:port/db
var mysqlUrl = FirstEnv("MYSQL_URL", "MYSQL_PUBLIC_URL");

var envHost = FirstEnv("DB_HOST", "MYSQLHOST");
var envPort = FirstEnv("DB_PORT", "MYSQLPORT");
var envName = FirstEnv("DB_NAME", "MYSQLDATABASE");
var envUser = FirstEnv("DB_USER", "MYSQLUSER");
var envPassword = FirstEnv("DB_PASSWORD", "MYSQLPASSWORD");

// Debug: variables detectadas (nunca imprimir el password)
Console.WriteLine(">>> ENV DB_HOST: " + (envHost ?? "NOT SET"));
Console.WriteLine(">>> ENV DB_PORT: " + (envPort ?? "NOT SET"));
Console.WriteLine(">>> ENV DB_NAME: " + (envName ?? "NOT SET"));
Console.WriteLine(">>> ENV DB_USER: " + (envUser ?? "NOT SET"));
Console.WriteLine(">>> ENV DB_PASSWORD: " + (string.IsNullOrWhiteSpace(envPassword) ? "NOT SET" : "SET"));
Console.WriteLine(">>> ENV MYSQL_URL: " + (string.IsNullOrWhiteSpace(mysqlUrl) ? "NOT SET" : "SET"));

var hasEnvConnection =
    !string.IsNullOrWhiteSpace(envHost) &&
    !string.IsNullOrWhiteSpace(envPort) &&
    !string.IsNullOrWhiteSpace(envName) &&
    !string.IsNullOrWhiteSpace(envUser) &&
    !string.IsNullOrWhiteSpace(envPassword);

string? connectionString;

if (hasEnvConnection)
{
    connectionString = $"server={envHost};port={envPort};database={envName};user={envUser};password={envPassword}";
}
else if (!string.IsNullOrWhiteSpace(mysqlUrl))
{
    // mysql://user:password@host:port/database
    var uri = new Uri(mysqlUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    connectionString = $"server={uri.Host};port={uri.Port};database={uri.AbsolutePath.TrimStart('/')};user={userInfo[0]};password={userInfo[1]}";
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("${"))
    throw new InvalidOperationException(
        "Missing or invalid database connection string. Set DB_HOST/DB_PORT/DB_NAME/DB_USER/DB_PASSWORD " +
        "(o las variables MYSQLHOST/MYSQLPORT/MYSQLDATABASE/MYSQLUSER/MYSQLPASSWORD, o MYSQL_URL) en Railway.");

// JWT secret from env var or appsettings
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("JWT secret is not configured.");

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySQL(connectionString!);
});

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, AppUnitOfWork>();

// Repositories
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IHuariqueRepository, HuariqueRepository>();
builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AddScoped<IPromoRepository, PromoRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Services
builder.Services.AddScoped<ITokenService, TokenService>();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

// Controllers + JSON
builder.Services.AddControllers().AddNewtonsoftJson();

// Swagger with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Bearer token. Ejemplo: Bearer {token}",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
    c.OperationFilter<PuntoSabor_Backend.Shared.Infrastructure.Swagger.FileUploadOperationFilter>();
});

const string corsPolicyName = "AllowHuariqueHubFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        policy.SetIsOriginAllowed(origin =>
              {
                  if (string.IsNullOrWhiteSpace(origin)) return false;
                  if (origin.StartsWith("http://localhost:", StringComparison.OrdinalIgnoreCase)) return true;
                  if (origin.Contains("huariquehub")) return true;
                  if (origin.Contains("puntosabor"))  return true;
                  if (origin.Contains("pflavor"))     return true;
                  return origin.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase);
              })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Console.WriteLine(">>> Verificando base de datos...");
        db.Database.EnsureCreated();

        // Crea tablas agregadas tras el despliegue inicial (no usa migraciones).
        Console.WriteLine(">>> Asegurando tablas adicionales...");
        SchemaInitializer.EnsureExtraTables(db);

        Console.WriteLine(">>> Ejecutando DataSeeder...");
        DataSeeder.Seed(db);
        Console.WriteLine(">>> DataSeeder terminado.");
    }
    catch (Exception ex)
    {
        Console.WriteLine(">>> ERROR AL CONECTAR A MYSQL EN STARTUP:");
        Console.WriteLine(ex.ToString());
        // No swallow: si la BD no esta lista, mejor que Railway marque el deploy
        // como fallido en vez de dejar el servicio "vivo" respondiendo 500 en todo.
        throw;
    }
}

app.UseCors(corsPolicyName);      // ← PRIMERO, antes de todo
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "PuntoSabor Backend"
}));
app.MapControllers();
app.Run();