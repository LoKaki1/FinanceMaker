
using FinanceMaker.Common.Extensions;
using FinanceMaker.Common.Models;
using FinanceMaker.Common.Models.Pullers;
using FinanceMaker.Pullers.TickerPullers.Interfaces;
using QuantConnect;

namespace FinanceMaker.Pullers.TickerPullers
{
	public sealed class FinvizTickersPuller: IParamtizedTickersPuller
	{
        private readonly IHttpClientFactory m_RequestService;
        private readonly string m_FinvizUrl;
        private readonly string m_FinvizStartSperator;
        private readonly string m_FinvizEndSeperator;

        public FinvizTickersPuller(IHttpClientFactory requestService)
        {
            m_RequestService = requestService;
            m_FinvizUrl = "https://finviz.com/screener.ashx??v=111";
            m_FinvizStartSperator = "</td></tr><!-- TS";
            m_FinvizEndSeperator = "TE -->";
        }

        public async Task<IEnumerable<string>> ScanTickers(TickersPullerParameters scannerParams, CancellationToken cancellationToken)
        {
            var httpClient = m_RequestService.CreateClient();
            httpClient.AddBrowserUserAgent();

            var url = string.Join("", m_FinvizUrl, GenerateParams(scannerParams));
            var finvizResult = await httpClient.GetAsync(url, cancellationToken);

            if (!finvizResult.IsSuccessStatusCode)
            {
                throw new NotSupportedException($"Something went wrong with finviz {finvizResult.RequestMessage}");
            }

            var finvizHtml = await finvizResult.Content.ReadAsStringAsync(cancellationToken);
            var onlyTickersData = finvizHtml.Split(m_FinvizStartSperator)[1]
                                            .Split(m_FinvizEndSeperator)[0]
                                            .Split("\n")
                                            .Select(tickerData => tickerData.Split("|")[0])
                                            .Where(ticker => !string.IsNullOrEmpty(ticker))
                                            .ToArray();

            return onlyTickersData;
        }

        private string GenerateParams(TickersPullerParameters scannerParams)
        {
            // TODO: Make it better (I'm sure you'll find a way)
            var maxPrice = scannerParams.MaxPrice;
            var minPrice = scannerParams.MinPrice;
            var maxAverageVolume = scannerParams.MaxAvarageVolume;
            var minAverageVolume = scannerParams.MinAvarageVolume;
            var minPresentOfChange = scannerParams.MinPresentageOfChange;
            var maxPresentageOfChange = scannerParams.MaxPresentageOfChange;
            //&f = sh_avgvol_100to1000,sh_price_u10,ta_change_u20,targetprice_a30 &
            var finvizParams = $"&f=sh_avgvol_{minAverageVolume / 1000}to{maxAverageVolume / 1000}," +
                $"sh_price_{minPrice}to{maxPrice},ta_change_{minPresentOfChange}to{maxPresentageOfChange}&ft=4";

            return finvizParams;
        }
    }
}

