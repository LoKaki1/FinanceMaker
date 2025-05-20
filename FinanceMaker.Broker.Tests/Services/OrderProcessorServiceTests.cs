using System;
using System.Threading.Tasks;
using FinanceMaker.Broker.Data;
using FinanceMaker.Broker.Models;
using FinanceMaker.Broker.Models.Responses;
using FinanceMaker.Broker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FinanceMaker.Broker.Tests.Services;

public class OrderProcessorServiceTests
{
    private readonly Mock<ILogger<OrderProcessorService>> m_LoggerMock;
    private readonly BrokerDbContext m_DbContext;
    private readonly OrderProcessorService m_Service;

    public OrderProcessorServiceTests()
    {
        m_LoggerMock = new Mock<ILogger<OrderProcessorService>>();
        var options = new DbContextOptionsBuilder<BrokerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        m_DbContext = new BrokerDbContext(options);
        m_Service = new OrderProcessorService(m_DbContext, m_LoggerMock.Object);
    }

    [Fact]
    public async Task ProcessOrderAsync_WithValidMarketOrder_ShouldSucceed()
    {
        // Arrange
        var user = new User { Username = "test", Email = "test@test.com" };
        var account = new Account { UserId = user.Id, Balance = 1000, AvailableFunds = 1000 };
        m_DbContext.Users.Add(user);
        m_DbContext.Accounts.Add(account);
        await m_DbContext.SaveChangesAsync();

        var order = new Order
        {
            AccountId = account.Id,
            Symbol = "AAPL",
            Type = OrderType.Market,
            Side = OrderSide.Buy,
            Quantity = 10,
            Status = OrderStatus.Pending
        };

        m_Service.UpdatePrice("AAPL", 100);

        // Act
        var result = await m_Service.ProcessOrderAsync(order);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(OrderStatus.Filled, order.Status);
        Assert.Equal(100, order.FilledPrice);
        Assert.NotNull(order.FilledAt);
        Assert.Equal(0, account.AvailableFunds); // 1000 - (10 * 100)
    }

    [Fact]
    public async Task ProcessOrderAsync_WithInsufficientFunds_ShouldFail()
    {
        // Arrange
        var user = new User { Username = "test", Email = "test@test.com" };
        var account = new Account { UserId = user.Id, Balance = 100, AvailableFunds = 100 };
        m_DbContext.Users.Add(user);
        m_DbContext.Accounts.Add(account);
        await m_DbContext.SaveChangesAsync();

        var order = new Order
        {
            AccountId = account.Id,
            Symbol = "AAPL",
            Type = OrderType.Market,
            Side = OrderSide.Buy,
            Quantity = 10,
            Status = OrderStatus.Pending
        };

        m_Service.UpdatePrice("AAPL", 100);

        // Act
        var result = await m_Service.ProcessOrderAsync(order);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Insufficient funds", result.Error);
        Assert.Equal(OrderStatus.Rejected, order.Status);
    }

    [Fact]
    public async Task ProcessOrderAsync_WithLimitOrder_ShouldExecuteWhenPriceMet()
    {
        // Arrange
        var user = new User { Username = "test", Email = "test@test.com" };
        var account = new Account { UserId = user.Id, Balance = 1000, AvailableFunds = 1000 };
        m_DbContext.Users.Add(user);
        m_DbContext.Accounts.Add(account);
        await m_DbContext.SaveChangesAsync();

        var order = new Order
        {
            AccountId = account.Id,
            Symbol = "AAPL",
            Type = OrderType.Limit,
            Side = OrderSide.Buy,
            Quantity = 10,
            LimitPrice = 90,
            Status = OrderStatus.Pending
        };

        m_Service.UpdatePrice("AAPL", 85); // Price below limit

        // Act
        var result = await m_Service.ProcessOrderAsync(order);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(OrderStatus.Filled, order.Status);
        Assert.Equal(85, order.FilledPrice);
    }

    [Fact]
    public async Task ProcessOrderAsync_WithStopOrder_ShouldExecuteWhenPriceMet()
    {
        // Arrange
        var user = new User { Username = "test", Email = "test@test.com" };
        var account = new Account { UserId = user.Id, Balance = 1000, AvailableFunds = 1000 };
        m_DbContext.Users.Add(user);
        m_DbContext.Accounts.Add(account);
        await m_DbContext.SaveChangesAsync();

        var order = new Order
        {
            AccountId = account.Id,
            Symbol = "AAPL",
            Type = OrderType.Stop,
            Side = OrderSide.Sell,
            Quantity = 10,
            StopPrice = 110,
            Status = OrderStatus.Pending
        };

        m_Service.UpdatePrice("AAPL", 115); // Price above stop

        // Act
        var result = await m_Service.ProcessOrderAsync(order);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(OrderStatus.Filled, order.Status);
        Assert.Equal(115, order.FilledPrice);
    }
}
