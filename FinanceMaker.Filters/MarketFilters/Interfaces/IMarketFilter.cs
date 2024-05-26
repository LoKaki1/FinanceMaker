using FinanceMaker.Common.Models;
using FinanceMaker.Common.Models.Filters;

namespace FinanceMaker.Filters.MarketFilters.Interfaces
{
    public interface IMarketFilter
    {
        Task<MarketFiltersResult> FilterTickersMarket(string ticker, IEnumerable<string> relatedTickers);
    }
}
