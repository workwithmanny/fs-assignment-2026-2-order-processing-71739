using MediatR;
using OrderManagement.API.Models;

namespace OrderManagement.API.CQRS.Queries;

public class GetOrderByIdQuery : IRequest<Order?>
{
    public Guid OrderId { get; set; }
}
