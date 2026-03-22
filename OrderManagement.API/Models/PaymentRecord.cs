namespace OrderManagement.API.Models;

public class PaymentRecord
{
    public int Id { get; set; }
    public Guid OrderId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public Order Order { get; set; } = null!;
}
