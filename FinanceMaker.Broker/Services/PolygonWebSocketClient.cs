using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using FinanceMaker.Broker.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinanceMaker.Broker.Services;

public class PolygonWebSocketClient : IDisposable
{
    private readonly ClientWebSocket m_WebSocket;
    private readonly ILogger<PolygonWebSocketClient> m_Logger;
    private readonly PolygonOptions m_Options;
    private readonly CancellationTokenSource m_CancellationTokenSource;
    private readonly HashSet<string> m_SubscribedSymbols;
    private readonly object m_Lock = new();

    public event EventHandler<PolygonTradeMessage>? OnTrade;
    public event EventHandler<PolygonQuoteMessage>? OnQuote;

    public PolygonWebSocketClient(
        ILogger<PolygonWebSocketClient> logger,
        IOptions<PolygonOptions> options)
    {
        m_Logger = logger;
        m_Options = options.Value;
        m_WebSocket = new ClientWebSocket();
        m_CancellationTokenSource = new CancellationTokenSource();
        m_SubscribedSymbols = new HashSet<string>();
    }

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            await m_WebSocket.ConnectAsync(new Uri(m_Options.WebSocketUrl), cancellationToken);
            m_Logger.LogInformation("Connected to Polygon.io WebSocket");

            // Start listening for messages
            _ = ListenForMessagesAsync(m_CancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Failed to connect to Polygon.io WebSocket");
            throw;
        }
    }

    public async Task SubscribeToSymbolsAsync(IEnumerable<string> symbols, CancellationToken cancellationToken)
    {
        var newSymbols = symbols.Where(s => !m_SubscribedSymbols.Contains(s)).ToList();
        if (!newSymbols.Any())
        {
            return;
        }

        var subscribeMessage = new
        {
            action = "subscribe",
            @params = $"T.{string.Join(",T.", newSymbols)}"
        };

        var message = JsonSerializer.Serialize(subscribeMessage);
        var buffer = Encoding.UTF8.GetBytes(message);

        await m_WebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cancellationToken);

        lock (m_Lock)
        {
            foreach (var symbol in newSymbols)
            {
                m_SubscribedSymbols.Add(symbol);
            }
        }

        m_Logger.LogInformation("Subscribed to symbols: {Symbols}", string.Join(", ", newSymbols));
    }

    private async Task ListenForMessagesAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await m_WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                ProcessMessage(message);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation, do nothing
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error while listening for WebSocket messages");
        }
    }

    private void ProcessMessage(string message)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(message);

            if (json.TryGetProperty("ev", out var eventType))
            {
                switch (eventType.GetString())
                {
                    case "T":
                        var trade = JsonSerializer.Deserialize<PolygonTradeMessage>(message);
                        if (trade != null)
                        {
                            OnTrade?.Invoke(this, trade);
                        }
                        break;

                    case "Q":
                        var quote = JsonSerializer.Deserialize<PolygonQuoteMessage>(message);
                        if (quote != null)
                        {
                            OnQuote?.Invoke(this, quote);
                        }
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error processing WebSocket message: {Message}", message);
        }
    }

    public void Dispose()
    {
        m_CancellationTokenSource.Cancel();
        m_WebSocket.Dispose();
        m_CancellationTokenSource.Dispose();
    }
}
