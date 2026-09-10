namespace PuntoSabor_Backend.Presentation.Resources;

/// Solicitud de recuperación de contraseña (US16).
public record ForgotPasswordResource(string Email);

/// Restablecimiento de contraseña (US16).
public record ResetPasswordResource(string Email, string NewPassword);
