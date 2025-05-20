using FinanceMaker.Broker.Interfaces;
using FinanceMaker.Broker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceMaker.Broker.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BrokerController : ControllerBase
{
    private readonly IBrokerService m_BrokerService;

    public BrokerController(IBrokerService brokerService)
    {
        m_BrokerService = brokerService;
    }

    [HttpPost("orders")]
    public async Task<ActionResult<OrderResult>> PlaceOrder([FromBody] Order order, CancellationToken cancellationToken)
    {
        var result = await m_BrokerService.PlaceOrderAsync(order, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("orders/{orderId}")]
    public async Task<ActionResult<OrderResult>> CancelOrder(string orderId, CancellationToken cancellationToken)
    {
        var result = await m_BrokerService.CancelOrderAsync(orderId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("positions/{symbol}")]
    public async Task<ActionResult<Position>> GetPosition(string symbol, CancellationToken cancellationToken)
    {
        var position = await m_BrokerService.GetPositionAsync(symbol, cancellationToken);
        return Ok(position);
    }

    [HttpGet("positions")]
    public async Task<ActionResult<IEnumerable<Position>>> GetAllPositions(CancellationToken cancellationToken)
    {
        var positions = await m_BrokerService.GetAllPositionsAsync(cancellationToken);
        return Ok(positions);
    }

    [HttpGet("account")]
    public async Task<ActionResult<AccountSummary>> GetAccountSummary(CancellationToken cancellationToken)
    {
        var summary = await m_BrokerService.GetAccountSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    [HttpGet("market-data/{symbol}")]
    public async Task<ActionResult<MarketData>> GetMarketData(string symbol, CancellationToken cancellationToken)
    {
        var marketData = await m_BrokerService.GetMarketDataAsync(symbol, cancellationToken);
        return Ok(marketData);
    }

    [HttpGet("orders")]
    public async Task<ActionResult<IEnumerable<Order>>> GetOpenOrders(CancellationToken cancellationToken)
    {
        var orders = await m_BrokerService.GetOpenOrdersAsync(cancellationToken);
        return Ok(orders);
    }

    [HttpGet("orders/{orderId}")]
    public async Task<ActionResult<Order>> GetOrder(string orderId, CancellationToken cancellationToken)
    {
        var order = await m_BrokerService.GetOrderAsync(orderId, cancellationToken);
        return Ok(order);
    }
}
