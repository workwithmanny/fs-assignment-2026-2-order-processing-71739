using MediatR;
using OrderManagement.API.CQRS.Commands;
using OrderManagement.API.Data;

namespace OrderManagement.API.CQRS.Handlers;

public class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusCommand, bool>
{
    private readonly AppDbContext _context;
    private readonly ILogger<UpdateOrderStatusHandler> _logger;

    public UpdateOrderStatusHandler(AppDbContext context, ILogger<UpdateOrderStatusHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FindAsync(request.OrderId);
        if (order == null) return false;

        order.Status = request.NewStatus;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} status updated to {Status}", order.Id, order.Status);
        return true;
    }
}
