using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FinanceMaker.Broker.Server.Controllers;
using FinanceMaker.Broker.Services;
using FinanceMaker.Broker.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace FinanceMaker.Tests.BrokerServer.Controllers;

public class AuthControllerTests : IClassFixture<WebApplicationFactory<AuthController>>
{
    private readonly WebApplicationFactory<AuthController> m_Factory;
    private readonly HttpClient m_Client;

    public AuthControllerTests(WebApplicationFactory<AuthController> factory)
    {
        m_Factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("environment", "Test");
            builder.ConfigureServices(services =>
            {
                // Remove the PolygonWebSocketClient registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(PolygonWebSocketClient));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                // Register AuthService as itself for controller resolution
                var authServiceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IAuthService));
                if (authServiceDescriptor != null)
                {
                    services.AddScoped<AuthService, AuthService>();
                }
                // Remove all DbContextOptions<BrokerDbContext> and BrokerDbContext registrations
                var dbContextOptionsDescriptors = services.Where(
                    d => d.ServiceType == typeof(DbContextOptions<FinanceMaker.Broker.Data.BrokerDbContext>)).ToList();
                foreach (var dbOptionsDescriptor in dbContextOptionsDescriptors)
                {
                    services.Remove(dbOptionsDescriptor);
                }
                var dbContextDescriptors = services.Where(
                    d => d.ServiceType == typeof(FinanceMaker.Broker.Data.BrokerDbContext)).ToList();
                foreach (var dbDescriptor in dbContextDescriptors)
                {
                    services.Remove(dbDescriptor);
                }
                services.AddDbContext<FinanceMaker.Broker.Data.BrokerDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });
        m_Client = m_Factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldSucceed()
    {
        var response = await m_Client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = "LoKaki",
            Email = "LoKaki@gmail.com",
            Password = "LoKaki"
        });

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldSucceed()
    {
        await m_Client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = "LoKaki",
            Email = "LoKaki@gmail.com",
            Password = "LoKaki"
        });

        var response = await m_Client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = "LoKaki",
            Password = "LoKaki"
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(result?.token);
    }
}
