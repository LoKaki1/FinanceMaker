using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using QuantConnect;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Interfaces;
using QuantConnect.Packets;
using Quotefeeder;

namespace FinanceMaker.BackTester.QCHelpers
{
    public class YahooLiveQuoteHandler : IDataQueueHandler
    {
        private readonly ConcurrentQueue<BaseData> _dataQueue = new();
        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cts;
        private Task _listenerTask;
        private readonly HashSet<string> _symbols = new();

        public bool IsConnected => _webSocket != null && _webSocket.State == WebSocketState.Open;

        public void Dispose()
        {
            _cts?.Cancel();
            _webSocket?.Dispose();
        }

        public void SetJob(LiveNodePacket job)
        {
            // Can be used to access job parameters if needed
        }

        public IEnumerator<BaseData> Subscribe(SubscriptionDataConfig dataConfig, EventHandler newDataAvailableHandler)
        {
            _symbols.Add(dataConfig.Symbol.Value);

            if (_webSocket == null)
            {
                _cts = new CancellationTokenSource();
                _webSocket = new ClientWebSocket();
                _listenerTask = Task.Run(() => StartListener(_cts.Token, newDataAvailableHandler));
            }
            else if (IsConnected)
            {
                var subscribeMessage = new { subscribe = _symbols }; // JSON: {"subscribe":["AAPL", "NIO"]}
                var json = JsonSerializer.Serialize(subscribeMessage);
                var bytes = Encoding.UTF8.GetBytes(json);
                _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, _cts.Token);
            }

            return _dataQueue.GetEnumerator();
        }

        public void Unsubscribe(SubscriptionDataConfig dataConfig)
        {
            _symbols.Remove(dataConfig.Symbol.Value);
        }

        private async Task StartListener(CancellationToken token, EventHandler newDataAvailableHandler)
        {
            try
            {
                await _webSocket.ConnectAsync(new Uri("wss://streamer.finance.yahoo.com/?version=2"), token);

                var subscribeMessage = new { subscribe = _symbols };
                var json = JsonSerializer.Serialize(subscribeMessage);
                var bytesToSend = Encoding.UTF8.GetBytes(json);
                await _webSocket.SendAsync(bytesToSend, WebSocketMessageType.Text, true, token);

                var buffer = new byte[4096];

                while (!token.IsCancellationRequested && _webSocket.State == WebSocketState.Open)
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                    if (result.MessageType == WebSocketMessageType.Close) break;

                    var message1 = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var jsonMessage = JsonSerializer.Deserialize<JsonElement>(message1);
                    var actualMessage = jsonMessage.GetProperty("message").GetString();
                    byte[] data = Convert.FromBase64String(actualMessage);

                    var msg = PricingData.Parser.ParseFrom(data);

                    var tick = new Tick
                    {
                        Symbol = Symbol.Create(msg.Id, SecurityType.Equity, Market.USA),
                        Time = DateTimeOffset.FromUnixTimeMilliseconds((long)msg.Time).UtcDateTime,
                        Value = (decimal)msg.Price,
                        BidPrice = (decimal)msg.Bid,
                        AskPrice = (decimal)msg.Ask,
                        TickType = TickType.Quote
                    };

                    _dataQueue.Enqueue(tick);
                    newDataAvailableHandler?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[YahooLiveQuoteHandler] Error: {ex.Message}");
            }
        }
    }
}
