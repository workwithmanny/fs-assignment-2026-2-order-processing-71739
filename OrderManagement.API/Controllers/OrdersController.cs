using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.API.CQRS.Commands;
using OrderManagement.API.CQRS.Queries;

namespace OrderManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutOrderCommand command)
    {
        var orderId = await _mediator.Send(command);
        return Ok(new { OrderId = orderId });
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _mediator.Send(new GetOrdersQuery());
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var order = await _mediator.Send(new GetOrderByIdQuery { OrderId = id });
        if (order == null) return NotFound();
        return Ok(order);
    }

    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetOrderStatus(Guid id)
    {
        var order = await _mediator.Send(new GetOrderByIdQuery { OrderId = id });
        if (order == null) return NotFound();
        return Ok(new { order.Id, order.Status });
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetCustomerOrders(Guid customerId)
    {
        var orders = await _mediator.Send(new GetCustomerOrdersQuery { CustomerId = customerId });
        return Ok(orders);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        var result = await _mediator.Send(new CancelOrderCommand { OrderId = id });
        if (!result) return NotFound();
        return Ok(new { message = "Order cancelled" });
    }
}
