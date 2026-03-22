using Shared.Contracts.DTOs;

namespace Shared.Contracts.Events;

public class OrderSubmittedEvent
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}
