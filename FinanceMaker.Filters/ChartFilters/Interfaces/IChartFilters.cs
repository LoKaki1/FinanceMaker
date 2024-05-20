using FinanceMaker.Common.Models.Filters;
using FinanceMaker.Common.Models.Tickers;

namespace FinanceMaker.Filters.ChartFilters.Interfaces
{
    public interface IChartFilters
    {
        Task<ChartFiltersResult> FilterTickersChart(TickerChart tickerChart);
    }
}
