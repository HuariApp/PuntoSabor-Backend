using Microsoft.EntityFrameworkCore;

namespace PuntoSabor_Backend.Shared.Infrastructure.Persistence.EFC;

/**
 * <summary>
 *     Crea de forma idempotente las tablas agregadas después del despliegue
 *     inicial. El proyecto usa <c>Database.EnsureCreated()</c> (no migraciones),
 *     por lo que las tablas nuevas no aparecen en una base ya existente. Estas
 *     sentencias <c>CREATE TABLE IF NOT EXISTS</c> las añaden sin afectar los
 *     datos actuales. Los nombres de columna coinciden con el mapeo de EF Core.
 * </summary>
 */
public static class SchemaInitializer
{
    // Agrega una columna solo si no existe (idempotente, seguro en cada arranque).
    private static void EnsureColumn(AppDbContext context, string table, string column, string definition)
    {
        var exists = context.Database
            .SqlQueryRaw<int>(
                "SELECT COUNT(*) AS Value FROM information_schema.COLUMNS " +
                "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = {0} AND COLUMN_NAME = {1}",
                table, column)
            .AsEnumerable()
            .First();

        if (exists == 0)
            context.Database.ExecuteSqlRaw($"ALTER TABLE `{table}` ADD COLUMN `{column}` {definition}");
    }
    public static void EnsureExtraTables(AppDbContext context)
    {
        context.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS `UserPreferences` (
                `Id` INT NOT NULL AUTO_INCREMENT,
                `UserId` INT NOT NULL,
                `PreferredCategory` VARCHAR(80) NULL,
                `MaxBudget` DECIMAL(18,2) NULL,
                `PreferredDistrict` VARCHAR(120) NULL,
                `NotificationsEnabled` TINYINT(1) NOT NULL DEFAULT 1,
                `CreatedAt` DATETIME(6) NOT NULL,
                `UpdatedAt` DATETIME(6) NULL,
                PRIMARY KEY (`Id`),
                UNIQUE KEY `IX_UserPreferences_UserId` (`UserId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
            """);

        context.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS `Reports` (
                `Id` INT NOT NULL AUTO_INCREMENT,
                `HuariqueId` INT NOT NULL,
                `UserId` INT NOT NULL,
                `Reason` VARCHAR(500) NOT NULL,
                `Status` VARCHAR(20) NOT NULL,
                `CreatedAt` DATETIME(6) NOT NULL,
                `UpdatedAt` DATETIME(6) NULL,
                PRIMARY KEY (`Id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
            """);

        context.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS `Notifications` (
                `Id` INT NOT NULL AUTO_INCREMENT,
                `UserId` INT NOT NULL,
                `Title` VARCHAR(120) NOT NULL,
                `Body` VARCHAR(500) NOT NULL,
                `IsRead` TINYINT(1) NOT NULL DEFAULT 0,
                `CreatedAt` DATETIME(6) NOT NULL,
                `UpdatedAt` DATETIME(6) NULL,
                PRIMARY KEY (`Id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
            """); 
        // Columnas de imagen en Huariques (la tabla ya existe, EnsureCreated no la altera).
        EnsureColumn(context, "Huariques", "ImageData", "LONGBLOB NULL");
        EnsureColumn(context, "Huariques", "ImageContentType", "VARCHAR(100) NULL");
    }
    
}
