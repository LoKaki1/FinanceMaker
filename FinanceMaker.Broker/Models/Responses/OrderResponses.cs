namespace FinanceMaker.Broker.Models.Responses;

public class OrderResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public Order? Order { get; set; }

    public static OrderResponse CreateSuccess(Order order) => new()
    {
        Success = true,
        Order = order
    };

    public static OrderResponse CreateError(string error) => new()
    {
        Success = false,
        Error = error
    };
}
