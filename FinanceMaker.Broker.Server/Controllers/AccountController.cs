using System.Security.Claims;
using FinanceMaker.Broker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceMaker.Broker.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly BrokerDbContext m_DbContext;

    public AccountController(BrokerDbContext dbContext)
    {
        m_DbContext = dbContext;
    }

    [HttpGet("portfolio")]
    public async Task<IActionResult> GetPortfolio(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var userAccount = await m_DbContext.Accounts
            .Include(account => account.Positions)
            .FirstOrDefaultAsync(account => account.UserId == userId, cancellationToken);

        if (userAccount is null)
        {
            return NotFound(new ErrorResponse("Account not found"));
        }

        var portfolioResponse = new PortfolioResponse
        {
            CashBalance = userAccount.CashBalance,
            AvailableFunds = userAccount.AvailableFunds,
            Positions = userAccount.Positions.Select(position => new PositionResponse
            {
                Symbol = position.Symbol,
                Quantity = position.Quantity,
                AveragePrice = position.AveragePrice,
                LastPrice = position.LastPrice,
                UnrealizedPnL = position.UnrealizedPnL
            }).ToList()
        };

        return Ok(portfolioResponse);
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] AmountRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var userAccount = await m_DbContext.Accounts
            .FirstOrDefaultAsync(account => account.UserId == userId, cancellationToken);

        if (userAccount is null)
        {
            return NotFound(new ErrorResponse("Account not found"));
        }

        if (request.Amount <= 0)
        {
            return BadRequest(new ErrorResponse("Amount must be positive"));
        }

        userAccount.CashBalance += request.Amount;
        userAccount.AvailableFunds += request.Amount;
        await m_DbContext.SaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] AmountRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var userAccount = await m_DbContext.Accounts
            .FirstOrDefaultAsync(account => account.UserId == userId, cancellationToken);

        if (userAccount is null)
        {
            return NotFound(new ErrorResponse("Account not found"));
        }

        if (request.Amount <= 0 || request.Amount > userAccount.AvailableFunds)
        {
            return BadRequest(new ErrorResponse("Invalid withdrawal amount"));
        }

        userAccount.CashBalance -= request.Amount;
        userAccount.AvailableFunds -= request.Amount;
        await m_DbContext.SaveChangesAsync(cancellationToken);
        return Ok();
    }

    private Guid GetUserId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdStr, out var userId) ? userId : Guid.Empty;
    }
}

public class AmountRequest
{
    public decimal Amount { get; set; }
}

public class ErrorResponse
{
    public string Error { get; }

    public ErrorResponse(string error)
    {
        Error = error;
    }
}

public class PortfolioResponse
{
    public decimal CashBalance { get; set; }
    public decimal AvailableFunds { get; set; }
    public List<PositionResponse> Positions { get; set; } = new();
}

public class PositionResponse
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal LastPrice { get; set; }
    public decimal UnrealizedPnL { get; set; }
}
