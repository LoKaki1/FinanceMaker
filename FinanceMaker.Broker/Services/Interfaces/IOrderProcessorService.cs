using FinanceMaker.Broker.Models;
using FinanceMaker.Broker.Models.Responses;

namespace FinanceMaker.Broker.Services.Interfaces;

public interface IOrderProcessorService
{
    Task<OrderResponse> ProcessOrderAsync(Order order);
    void UpdatePrice(string symbol, decimal price);
}
