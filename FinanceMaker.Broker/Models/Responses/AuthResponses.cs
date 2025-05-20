namespace FinanceMaker.Broker.Models.Responses;

public class AuthResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? Token { get; set; }

    public static AuthResponse CreateSuccess(string token) => new()
    {
        Success = true,
        Token = token
    };

    public static AuthResponse CreateError(string error) => new()
    {
        Success = false,
        Error = error
    };
}
