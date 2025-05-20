using FinanceMaker.Broker.Models.Responses;

namespace FinanceMaker.Broker.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(string username, string email, string password);
    Task<AuthResponse> LoginAsync(string username, string password);
}
