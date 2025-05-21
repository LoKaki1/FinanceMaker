using System;
using System.Threading;
using System.Threading.Tasks;
using FinanceMaker.Broker.Interfaces;
using FinanceMaker.Broker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FinanceMaker.Broker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly ILogger<OrdersController> m_Logger;
        private readonly ICacheService m_CacheService;
        private readonly IRetryPolicyService m_RetryPolicyService;

        public OrdersController(
            ILogger<OrdersController> logger,
            ICacheService cacheService,
            IRetryPolicyService retryPolicyService)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_CacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            m_RetryPolicyService = retryPolicyService ?? throw new ArgumentNullException(nameof(retryPolicyService));
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var cacheKey = $"order_{request.Symbol}_{DateTime.UtcNow:yyyyMMdd}";

                // Check if we have a cached order for this symbol today
                var cachedOrder = await m_CacheService.GetAsync<OrderRequest>(cacheKey, cancellationToken);
                if (cachedOrder != null)
                {
                    return BadRequest("An order for this symbol has already been placed today");
                }

                // Execute order creation with retry policy
                var order = await m_RetryPolicyService.ExecuteWithRetryAsync(async (ct) =>
                {
                    // TODO: Implement actual order creation logic here
                    return new Order
                    {
                        Id = Guid.NewGuid(),
                        Symbol = request.Symbol,
                        Quantity = request.Quantity,
                        Price = request.Price,
                        Status = OrderStatus.Pending,
                        CreatedAt = DateTime.UtcNow
                    };
                }, cancellationToken);

                // Cache the order request
                await m_CacheService.SetAsync(cacheKey, request, TimeSpan.FromHours(24), cancellationToken);

                return Ok(order);
            }
            catch (Exception ex)
            {
                m_Logger.LogError(ex, "Error creating order for symbol {Symbol}", request.Symbol);
                return StatusCode(500, "An error occurred while processing your order");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var cacheKey = $"order_status_{id}";

                // Try to get order status from cache first
                var cachedOrder = await m_CacheService.GetAsync<Order>(cacheKey, cancellationToken);
                if (cachedOrder != null)
                {
                    return Ok(cachedOrder);
                }

                // Execute order retrieval with retry policy
                var order = await m_RetryPolicyService.ExecuteWithRetryAsync(async (ct) =>
                {
                    // TODO: Implement actual order retrieval logic here
                    return new Order
                    {
                        Id = id,
                        Symbol = "AAPL",
                        Quantity = 100,
                        Price = 150.00m,
                        Status = OrderStatus.Completed,
                        CreatedAt = DateTime.UtcNow
                    };
                }, cancellationToken);

                // Cache the order status
                await m_CacheService.SetAsync(cacheKey, order, TimeSpan.FromMinutes(5), cancellationToken);

                return Ok(order);
            }
            catch (Exception ex)
            {
                m_Logger.LogError(ex, "Error retrieving order {OrderId}", id);
                return StatusCode(500, "An error occurred while retrieving the order");
            }
        }
    }
}
