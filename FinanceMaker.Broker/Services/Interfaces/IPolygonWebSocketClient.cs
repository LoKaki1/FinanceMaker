namespace FinanceMaker.Broker.Services.Interfaces;

public interface IPolygonWebSocketClient
{
    Task ConnectAsync(CancellationToken cancellationToken);
    Task DisconnectAsync(CancellationToken cancellationToken);
    Task SubscribeAsync(string symbol, CancellationToken cancellationToken);
    Task UnsubscribeAsync(string symbol, CancellationToken cancellationToken);
}
