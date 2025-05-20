using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FinanceMaker.Broker.Data;
using FinanceMaker.Broker.Models;
using FinanceMaker.Broker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace FinanceMaker.Tests.Broker.Services;

public class AuthServiceTests
{
    private readonly AuthService m_AuthService;
    private readonly BrokerDbContext m_DbContext;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<BrokerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        m_DbContext = new BrokerDbContext(options);
        var jwtOptions = new JwtOptions
        {
            Secret = "YourSuperSecretKey12345678901234567890", // At least 32 characters for 256 bits
            Issuer = "FinanceMaker",
            Audience = "FinanceMakerUsers",
            ExpiryInMinutes = 60
        };

        m_AuthService = new AuthService(m_DbContext, Options.Create(jwtOptions));
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldSucceed()
    {
        // Arrange
        var username = "testuser";
        var email = "test@example.com";
        var password = "password123";

        // Act
        var result = await m_AuthService.RegisterAsync(username, email, password);

        // Assert
        Assert.True(result.Success);
        var user = await m_DbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
        Assert.NotNull(user);
        Assert.Equal(email, user.Email);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ShouldFail()
    {
        // Arrange
        var username = "testuser";
        var email = "test@example.com";
        var password = "password123";

        await m_AuthService.RegisterAsync(username, email, password);

        // Act
        var result = await m_AuthService.RegisterAsync(username, "another@example.com", "password456");

        // Assert
        Assert.False(result.Success);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldSucceed()
    {
        // Arrange
        var username = "testuser";
        var email = "test@example.com";
        var password = "password123";

        await m_AuthService.RegisterAsync(username, email, password);

        // Act
        var result = await m_AuthService.LoginAsync(username, password);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Token);
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("YourSuperSecretKey12345678901234567890");
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = "FinanceMaker",
            ValidateAudience = true,
            ValidAudience = "FinanceMakerUsers",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var principal = tokenHandler.ValidateToken(result.Token, tokenValidationParameters, out var validatedToken);
        var jwtToken = (JwtSecurityToken)validatedToken;
        // Print all claims for debugging
        foreach (var claim in jwtToken.Claims)
        {
            Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
        }
        // Check for the 'unique_name' claim
        var usernameClaim = jwtToken.Claims.First(x => x.Type == "unique_name").Value;
        Assert.Equal(username, usernameClaim);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ShouldFail()
    {
        // Arrange
        var username = "testuser";
        var email = "test@example.com";
        var password = "password123";

        await m_AuthService.RegisterAsync(username, email, password);

        // Act
        var result = await m_AuthService.LoginAsync(username, "wrongpassword");

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Token);
    }
}
