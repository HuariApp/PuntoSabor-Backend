namespace PuntoSabor_Backend.Presentation.Resources;

public record FavoriteResource(
    int Id,
    int UserId,
    int HuariqueId,
    DateTime CreatedAt);
