using MediatR;
using Shared.Contracts.Enums;

namespace OrderManagement.API.CQRS.Commands;

public class UpdateOrderStatusCommand : IRequest<bool>
{
    public Guid OrderId { get; set; }
    public OrderStatus NewStatus { get; set; }
}
