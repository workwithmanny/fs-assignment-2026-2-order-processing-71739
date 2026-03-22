using MediatR;
using OrderManagement.API.Models;
using Shared.Contracts.Enums;

namespace OrderManagement.API.CQRS.Queries;

public class GetOrdersByStatusQuery : IRequest<List<Order>>
{
    public OrderStatus Status { get; set; }
}
