namespace Shared.Contracts.Events;

public class ShippingCreatedEvent
{
    public Guid OrderId { get; set; }
    public string ShipmentReference { get; set; } = string.Empty;
    public DateTime EstimatedDispatchDate { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

public class ShippingFailedEvent
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
}
