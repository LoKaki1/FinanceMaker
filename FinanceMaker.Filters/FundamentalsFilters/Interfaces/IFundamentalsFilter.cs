using FinanceMaker.Common.Models;
using FinanceMaker.Common.Models.Filters;
using FinanceMaker.Common.Models.Tickers;

namespace FinanceMaker.Filters.FundamentalsFilters.Interfaces
{
    public interface IFundamentalsFilter
    {
        Task<NewsFilterResult> FilterTickerFundamentals(TickerNews tickerNews);
    }
}
