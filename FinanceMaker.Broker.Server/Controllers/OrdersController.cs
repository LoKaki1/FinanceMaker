using System.Security.Claims;
using FinanceMaker.Broker.Models.Enums;
using FinanceMaker.Broker.Models.Requests;
using FinanceMaker.Broker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceMaker.Broker.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService m_OrderService;

    public OrdersController(IOrderService orderService)
    {
        m_OrderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var order = await m_OrderService.CreateOrderAsync(
            Guid.Parse(userId),
            request.Symbol,
            request.Type,
            request.Side,
            request.Quantity,
            request.Price,
            request.TakeProfitPrice,
            request.StopLossPrice,
            cancellationToken);

        return Ok(order);
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var orders = await m_OrderService.GetOrdersAsync(Guid.Parse(userId), cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var order = await m_OrderService.GetOrderAsync(id, Guid.Parse(userId), cancellationToken);
        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await m_OrderService.CancelOrderAsync(id, Guid.Parse(userId), cancellationToken);
        if (!result)
        {
            return NotFound();
        }

        return Ok();
    }
}
