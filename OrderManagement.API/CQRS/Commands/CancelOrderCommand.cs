using MediatR;

namespace OrderManagement.API.CQRS.Commands;

public class CancelOrderCommand : IRequest<bool>
{
    public Guid OrderId { get; set; }
}
