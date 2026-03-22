using MediatR;
using Shared.Contracts.DTOs;

namespace OrderManagement.API.CQRS.Commands;

public class CheckoutOrderCommand : IRequest<Guid>
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();
}
