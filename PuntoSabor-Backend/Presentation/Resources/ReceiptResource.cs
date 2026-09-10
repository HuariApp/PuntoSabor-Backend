namespace PuntoSabor_Backend.Presentation.Resources;

/// Comprobante de pago de una suscripción (US25).
public record ReceiptResource(
    string ReceiptNumber,
    int SubscriptionId,
    int UserId,
    string PlanId,
    string PlanName,
    decimal Amount,
    string Currency,
    string Status,
    DateTime IssuedAt,
    DateTime PeriodStart,
    DateTime? PeriodEnd
);
