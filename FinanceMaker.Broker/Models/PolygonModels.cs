namespace FinanceMaker.Broker.Models;

public class PolygonOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string WebSocketUrl { get; set; } = string.Empty;
    public int MaxReconnectAttempts { get; set; }
    public int ReconnectDelayMs { get; set; }
    public int MaxRequestsPerMinute { get; set; }
}

public class PolygonTradeMessage
{
    public string Ev { get; set; } = string.Empty;
    public string Sym { get; set; } = string.Empty;
    public decimal P { get; set; }
    public int S { get; set; }
    public long T { get; set; }
}

public class PolygonQuoteMessage
{
    public string Ev { get; set; } = string.Empty;
    public string Sym { get; set; } = string.Empty;
    public decimal Bp { get; set; }
    public decimal Ap { get; set; }
    public int Bs { get; set; }
    public int As { get; set; }
    public long T { get; set; }
}
