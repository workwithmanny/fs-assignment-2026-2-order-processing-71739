using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderManagement.API.CQRS.Queries;
using OrderManagement.API.Data;
using OrderManagement.API.Models;

namespace OrderManagement.API.CQRS.Handlers;

public class GetOrdersHandler : IRequestHandler<GetOrdersQuery, List<Order>>
{
    private readonly AppDbContext _context;

    public GetOrdersHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Order>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Payment)
            .Include(o => o.Shipment)
            .ToListAsync(cancellationToken);
    }
}
