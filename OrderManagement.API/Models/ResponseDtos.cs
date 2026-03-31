namespace OrderManagement.API.Models;

public class OrderResponseDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public List<OrderItemResponseDto> Items { get; set; } = new();
    public PaymentResponseDto? Payment { get; set; }
    public ShipmentResponseDto? Shipment { get; set; }
}

public class OrderItemResponseDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class PaymentResponseDto
{
    public string TransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
    public DateTime ProcessedAt { get; set; }
}

public class ShipmentResponseDto
{
    public string ShipmentReference { get; set; } = string.Empty;
    public DateTime EstimatedDispatchDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
