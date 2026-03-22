namespace Shared.Contracts.Events;

public class InventoryConfirmedEvent
{
    public Guid OrderId { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

public class InventoryFailedEvent
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
}
