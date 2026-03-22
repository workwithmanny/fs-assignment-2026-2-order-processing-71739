using MediatR;
using OrderManagement.API.Models;

namespace OrderManagement.API.CQRS.Queries;

public class GetOrdersQuery : IRequest<List<Order>>
{
}
