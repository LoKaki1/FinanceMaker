using System.Security.Claims;
using FinanceMaker.Broker.Data;
using FinanceMaker.Broker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceMaker.Broker.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly BrokerDbContext m_DbContext;

    public OrdersController(BrokerDbContext dbContext)
    {
        m_DbContext = dbContext;
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var userAccount = await m_DbContext.Accounts
            .FirstOrDefaultAsync(account => account.UserId == userId, cancellationToken);

        if (userAccount is null)
        {
            return NotFound(new { error = "Account not found" });
        }

        if (request.Quantity <= 0 || string.IsNullOrWhiteSpace(request.Symbol))
        {
            return BadRequest(new { error = "Invalid order request" });
        }

        var newOrder = new Order
        {
            AccountId = userAccount.Id,
            Symbol = request.Symbol,
            Type = request.Type,
            Side = request.Side,
            Quantity = request.Quantity,
            LimitPrice = request.LimitPrice,
            StopPrice = request.StopPrice,
            TakeProfitPrice = request.TakeProfitPrice,
            StopLossPrice = request.StopLossPrice,
            Status = OrderStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        m_DbContext.Orders.Add(newOrder);
        await m_DbContext.SaveChangesAsync(cancellationToken);
        return Ok(newOrder);
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var userOrders = await m_DbContext.Orders
            .Include(order => order.Account)
            .Where(order => order.Account.UserId == userId)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(userOrders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var userOrder = await m_DbContext.Orders
            .Include(order => order.Account)
            .FirstOrDefaultAsync(order => order.Id == id && order.Account.UserId == userId, cancellationToken);

        if (userOrder is null)
        {
            return NotFound(new { error = "Order not found" });
        }

        return Ok(userOrder);
    }

    private Guid GetUserId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdStr, out var userId) ? userId : Guid.Empty;
    }
}

public class PlaceOrderRequest
{
    public string Symbol { get; set; } = string.Empty;
    public OrderType Type { get; set; }
    public OrderSide Side { get; set; }
    public int Quantity { get; set; }
    public decimal? LimitPrice { get; set; }
    public decimal? StopPrice { get; set; }
    public decimal? TakeProfitPrice { get; set; }
    public decimal? StopLossPrice { get; set; }
}
