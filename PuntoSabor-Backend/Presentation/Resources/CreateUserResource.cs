namespace PuntoSabor_Backend.Presentation.Resources;

/**
 * <summary>
 *     Datos necesarios para crear un usuario.
 * </summary>
 */

public record CreateUserResource(
    string Name,
    string Email,
    string Password,
    string Role = "consumer");