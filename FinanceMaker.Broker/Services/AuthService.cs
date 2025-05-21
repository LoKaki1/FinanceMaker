using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FinanceMaker.Broker.Data;
using FinanceMaker.Broker.Models;
using FinanceMaker.Broker.Models.Responses;
using FinanceMaker.Broker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FinanceMaker.Broker.Services;

public class AuthService : IAuthService
{
    private readonly BrokerDbContext m_DbContext;
    private readonly JwtOptions m_JwtOptions;

    public AuthService(BrokerDbContext dbContext, IOptions<JwtOptions> jwtOptions)
    {
        m_DbContext = dbContext;
        m_JwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResponse> RegisterAsync(string username, string email, string password)
    {
        if (await m_DbContext.Users.AnyAsync(user => user.Username == username))
        {
            return AuthResponse.CreateError("Username already exists");
        }

        if (await m_DbContext.Users.AnyAsync(user => user.Email == email))
        {
            return AuthResponse.CreateError("Email already exists");
        }

        var newUser = new User
        {
            Username = username,
            Email = email,
            PasswordHash = HashPassword(password)
        };

        m_DbContext.Users.Add(newUser);
        await m_DbContext.SaveChangesAsync();

        // Create account for the user
        var newAccount = new Account
        {
            UserId = newUser.Id,
            CashBalance = 0,
            AvailableFunds = 0
        };

        m_DbContext.Accounts.Add(newAccount);
        await m_DbContext.SaveChangesAsync();

        var token = GenerateJwtToken(newUser);
        return AuthResponse.CreateSuccess(token);
    }

    public async Task<AuthResponse> LoginAsync(string username, string password)
    {
        var user = await m_DbContext.Users
            .FirstOrDefaultAsync(user => user.Username == username);

        if (user == null)
        {
            return AuthResponse.CreateError("Invalid credentials");
        }

        if (!VerifyPassword(password, user.PasswordHash))
        {
            return AuthResponse.CreateError("Invalid credentials");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await m_DbContext.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        return AuthResponse.CreateSuccess(token);
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(m_JwtOptions.Secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            }),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature),
            Issuer = m_JwtOptions.Issuer,
            Audience = m_JwtOptions.Audience
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPassword(string password, string hash)
    {
        var hashedPassword = HashPassword(password);
        return hashedPassword == hash;
    }
}
