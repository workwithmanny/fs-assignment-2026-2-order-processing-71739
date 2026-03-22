namespace Shared.Contracts.Events;

public class PaymentApprovedEvent
{
    public Guid OrderId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
}

public class PaymentRejectedEvent
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
}
