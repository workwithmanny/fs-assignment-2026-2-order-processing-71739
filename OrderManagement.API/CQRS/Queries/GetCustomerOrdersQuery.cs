using MediatR;
using OrderManagement.API.Models;

namespace OrderManagement.API.CQRS.Queries;

public class GetCustomerOrdersQuery : IRequest<List<Order>>
{
    public Guid CustomerId { get; set; }
}
