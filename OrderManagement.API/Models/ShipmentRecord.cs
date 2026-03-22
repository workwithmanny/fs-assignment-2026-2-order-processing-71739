namespace OrderManagement.API.Models;

public class ShipmentRecord
{
    public int Id { get; set; }
    public Guid OrderId { get; set; }
    public string ShipmentReference { get; set; } = string.Empty;
    public DateTime EstimatedDispatchDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Order Order { get; set; } = null!;
}
