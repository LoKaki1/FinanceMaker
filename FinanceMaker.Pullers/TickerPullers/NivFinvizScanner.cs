using FinanceMaker.Common.Models.Pullers;

namespace FinanceMaker.Pullers.TickerPullers;

public class NivTickersPuller : FinvizTickersPuller
{
    public NivTickersPuller(IHttpClientFactory requestService) : base(requestService)
    {
        m_FinvizUrl = "https://finviz.com/screener.ashx?v=111&s=ta_topgainers&f=sh_curvol_o1000,sh_price_o3,sh_relvol_o1&ta=0";
    }

    public override async Task<IEnumerable<string>> ScanTickers(TickersPullerParameters scannerParams, CancellationToken cancellationToken)
    {
        // Stop trying removin' the await, it's not gonna work
        // The c# can't convert Task<IEnumerable<string>> to Task<string[]>
        // But I think this docummentation will recover your memory 
        // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/ 
        // (It's just from the copilot and it looks cool don't really click on it)
        return await GetTickers(m_FinvizUrl, cancellationToken);
    }
}
