using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinanceMaker.Common.Models.Pullers.Enums;

namespace FinanceMaker.Broker.Interfaces;

public interface IMarketDataService
{
    Task<decimal> GetLastPriceAsync(string symbol, CancellationToken cancellationToken = default);
    Task<Dictionary<string, decimal>> GetLastPricesAsync(IEnumerable<string> symbols, CancellationToken cancellationToken = default);
    Task StartMarketDataUpdatesAsync(CancellationToken cancellationToken = default);
}
