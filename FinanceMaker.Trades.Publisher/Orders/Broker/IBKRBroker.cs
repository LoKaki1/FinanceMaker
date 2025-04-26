using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FinanceMaker.Common.Models.Ideas.Enums;
using FinanceMaker.Common.Models.Ideas.IdeaOutputs;
using FinanceMaker.Common.Models.Interactive;
using FinanceMaker.Common.Models.Trades.Enums;
using FinanceMaker.Common.Models.Trades.Trader;
using FinanceMaker.Publisher.Orders.Trader.Abstracts;
using FinanceMaker.Publisher.Orders.Trader.Interfaces;
using FinanceMaker.Publisher.Orders.Trades;
using ITrade = FinanceMaker.Trades.Publisher.Orders.Trades.Interfaces.ITrade;

namespace FinanceMaker.Publisher.Orders.Broker;

public class IBKRBroker : BrokerrBase<EntryExitOutputIdea>
{
    private readonly IHttpClientFactory m_HttpClientFactory;
    private readonly HttpClientHandler m_Handler;
    private readonly string m_BaseUrl;
    private readonly JsonSerializerOptions m_JsonOptions;
    private string? m_SessionId;

    public override TraderType Type => TraderType.EntryExit | TraderType.StopLoss;

    public IBKRBroker(IHttpClientFactory httpClientFactory)
    {

        m_HttpClientFactory = httpClientFactory;
        m_Handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        // The base URL should point to your local Client Portal Gateway instance
        m_BaseUrl = "https://localhost:5001/v1/api";
        m_JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    private void ConfigureClientHeaders(HttpClient client)
    {
        client.DefaultRequestHeaders.Add("User-Agent", "FinanceMaker/1.0");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        client.DefaultRequestHeaders.Add("Host", "api.ibkr.com");
    }

    private async Task EnsureAuthenticated(CancellationToken cancellationToken)
    {
        try
        {
            if (!string.IsNullOrEmpty(m_SessionId))
                return;

            var client = m_HttpClientFactory.CreateClient("IBKR");
            ConfigureClientHeaders(client);

            // First validate if we're already authenticated
            var statusResponse = await client.GetAsync($"{m_BaseUrl}/iserver/auth/status", cancellationToken);
            var statusResult = await statusResponse.Content.ReadFromJsonAsync<IBKRAuthResponse>(m_JsonOptions, cancellationToken);
            var statusResult1 = await statusResponse.Content.ReadAsStringAsync(cancellationToken);

            if (statusResult?.Authenticated == true)
            {
                m_SessionId = statusResponse.Headers.GetValues("X-IB-Session").FirstOrDefault();
                return;
            }

            // If not authenticated, try to authenticate
            var payload = new
            {
                publish = true,
                compete = true
            };

            string jsonString = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var authResponse = await client.PostAsync($"{m_BaseUrl}/iserver/auth/ssodh/init", content, cancellationToken);
            var errorContenta = await authResponse.Content.ReadAsStringAsync(cancellationToken);
            if (!authResponse.IsSuccessStatusCode)
            {
                var errorContent = await authResponse.Content.ReadAsStringAsync(cancellationToken);
                throw new Exception($"Authentication failed: {errorContent}");
            }

            var authResult = await authResponse.Content.ReadFromJsonAsync<IBKRAuthResponse>(m_JsonOptions, cancellationToken);
            var authResult1 = await authResponse.Content.ReadAsStringAsync(cancellationToken);

            if (authResult?.Authenticated == true)
            {
                m_SessionId = authResponse.Headers.GetValues("X-IB-Session").FirstOrDefault();
                // Validate the session
                await client.PostAsync($"{m_BaseUrl}/iserver/auth/validateSession", null, cancellationToken);
            }
            else
            {
                throw new Exception("Failed to authenticate with IBKR Client Portal. Make sure the Client Portal Gateway is running and you're logged in.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error during authentication: {ex.Message}", ex);
        }

    }

    protected override async Task<ITrade> TradeInternal(EntryExitOutputIdea idea, CancellationToken cancellationToken)
    {
        await EnsureAuthenticated(cancellationToken);

        var client = m_HttpClientFactory.CreateClient("IBKR");
        ConfigureClientHeaders(client);

        var orderRequest = new IBKROrderRequest
        {
            ConId = await GetContractId(idea.Ticker, cancellationToken),
            Side = idea.Trade == IdeaTradeType.Long ? "BUY" : "SELL",
            Quantity = idea.Quantity,
            Price = (decimal)idea.Entry,
            StopPrice = (decimal)idea.Stoploss,
            TakeProfit = (decimal)idea.Exit,
            OrderType = "LMT",
            Tif = "GTC",
            OutsideRth = true
        };

        var json = JsonSerializer.Serialize(orderRequest, m_JsonOptions);
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        using var response = await client.PostAsync($"{m_BaseUrl}/iserver/account/orders", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"Order placement failed: {errorContent}");
        }

        var result = await response.Content.ReadFromJsonAsync<IBKROrderResponse>(m_JsonOptions, cancellationToken);

        if (!string.IsNullOrEmpty(result?.OrderId))
        {
            return new Trade(idea, Guid.Parse(result.OrderId), true);
        }

        return Trade.Empty;
    }

    private async Task<string> GetContractId(string symbol, CancellationToken cancellationToken)
    {
        var client = m_HttpClientFactory.CreateClient("IBKR");
        ConfigureClientHeaders(client);

        var response = await client.GetAsync($"{m_BaseUrl}/iserver/secdef/search?symbol={Uri.EscapeDataString(symbol)}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"Contract search failed: {errorContent}");
        }

        var results = await response.Content.ReadFromJsonAsync<List<IBKRContract>>(m_JsonOptions, cancellationToken);

        if (results?.Count > 0)
        {
            return results[0].ConId;
        }

        throw new Exception($"Could not find contract ID for symbol: {symbol}");
    }

    public override async Task<Position> GetClientPosition(CancellationToken cancellationToken)
    {
        await EnsureAuthenticated(cancellationToken);

        var client = m_HttpClientFactory.CreateClient("IBKR");
        client.DefaultRequestHeaders.Add("X-IB-Session", m_SessionId);

        var accountResponse = await client.GetAsync($"{m_BaseUrl}/portfolio/accounts", cancellationToken);
        var accounts = await accountResponse.Content.ReadFromJsonAsync<List<IBKRAccountSummary>>(cancellationToken);

        if (accounts?.Count == 0)
            throw new Exception("No accounts found");

        var accountId = accounts[0].Id;

        var positionsResponse = await client.GetAsync($"{m_BaseUrl}/portfolio/{accountId}/positions", cancellationToken);
        var positions = await positionsResponse.Content.ReadFromJsonAsync<List<IBKRPosition>>(cancellationToken);

        var ordersResponse = await client.GetAsync($"{m_BaseUrl}/iserver/account/orders", cancellationToken);
        var orders = await ordersResponse.Content.ReadFromJsonAsync<List<IBKROrderResponse>>(cancellationToken);

        return new Position
        {
            BuyingPower = (float)accounts[0].BuyingPower,
            OpenedPositions = positions?.Select(p => p.Symbol).ToArray() ?? Array.Empty<string>(),
            Orders = orders?.Select(o => o.Symbol).ToArray() ?? Array.Empty<string>()
        };
    }

    public override async Task CancelTrade(ITrade trade, CancellationToken cancellationToken)
    {
        await EnsureAuthenticated(cancellationToken);

        var client = m_HttpClientFactory.CreateClient("IBKR");
        client.DefaultRequestHeaders.Add("X-IB-Session", m_SessionId);

        await client.DeleteAsync($"{m_BaseUrl}/iserver/account/order/{trade.TradeId}", cancellationToken);
        await trade.Cancel(cancellationToken);
    }
}
