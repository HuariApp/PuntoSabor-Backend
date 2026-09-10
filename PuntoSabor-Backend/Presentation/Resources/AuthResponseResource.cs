namespace PuntoSabor_Backend.Presentation.Resources;

public record AuthResponseResource(
    int Id,
    string Name,
    string Email,
    string Role,
    string Token);
