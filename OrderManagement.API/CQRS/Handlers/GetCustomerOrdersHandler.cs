using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderManagement.API.CQRS.Queries;
using OrderManagement.API.Data;
using OrderManagement.API.Models;

namespace OrderManagement.API.CQRS.Handlers;

public class GetCustomerOrdersHandler : IRequestHandler<GetCustomerOrdersQuery, List<Order>>
{
    private readonly AppDbContext _context;

    public GetCustomerOrdersHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Order>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Payment)
            .Include(o => o.Shipment)
            .Where(o => o.CustomerId == request.CustomerId)
            .ToListAsync(cancellationToken);
    }
}
