using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderManagement.API.CQRS.Queries;
using OrderManagement.API.Data;
using OrderManagement.API.Models;

namespace OrderManagement.API.CQRS.Handlers;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, Order?>
{
    private readonly AppDbContext _context;

    public GetOrderByIdHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Payment)
            .Include(o => o.Shipment)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);
    }
}
