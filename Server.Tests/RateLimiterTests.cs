using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using Shouldly;
using System.Net;

namespace Server.Tests;

public class RateLimiterTests : IDisposable
{
    private readonly TestServer _server;
    private readonly HttpClient _client;
    private readonly FakeTimeProvider _fakeTimeProvider;

    public RateLimiterTests()
    {
        _fakeTimeProvider = new FakeTimeProvider();
        _server = new TestServer(
            new WebHostBuilder()
                .UseStartup<Startup>()
                .ConfigureTestServices(services =>
                {
                    services.Replace(ServiceDescriptor.Singleton<TimeProvider>(_fakeTimeProvider));
                })
        );
        _client = _server.CreateClient();
    }

    public void Dispose()
    {
        _server.Dispose();
        _client.Dispose();
    }

    [Fact]
    public async Task SingleClientShouldReceive503StatusAfterExceedingLimit()
    {
        // Arrange
        _fakeTimeProvider.SetUtcNow(new DateTimeOffset(new DateTime(2023, 10, 2)));
        for (var i = 0; i < 4; i++)
        {
            var response = await _client.GetAsync("/?clientId=1");
            response.EnsureSuccessStatusCode();
        }

        // Act
        var statusCodes = Enumerable.Range(0, 10) // Every request after 4th one gets 503
            .Select(_ => _client.GetAsync("/?clientId=1").Result.StatusCode)
            .ToArray();

        // Assert
        statusCodes.ShouldAllBe(code => code == HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task TimeFrameIsResetAfter5Seconds()
    {
        // Arrange
        _fakeTimeProvider.SetUtcNow(new DateTimeOffset(new DateTime(2023, 10, 2)));
        for (var i = 0; i < 4; i++)
        {
            var response = await _client.GetAsync("/?clientId=1");
            response.EnsureSuccessStatusCode();
        }

        var limitedResponse = await _client.GetAsync("/?clientId=1");
        Assert.Equal(limitedResponse.StatusCode, HttpStatusCode.ServiceUnavailable);

        _fakeTimeProvider.SetUtcNow(new DateTimeOffset(new DateTime(2023, 10, 2) + TimeSpan.FromSeconds(6)));

        // Act
        var newResponse = await _client.GetAsync("/?clientId=1");

        // Assert
        newResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DifferentClientIdsDoNotInterfereWithOneAnother()
    {
        // Arrange
        var clientId = 1;
        var otherClientId = 2;
        _fakeTimeProvider.SetUtcNow(new DateTimeOffset(new DateTime(2023, 10, 2)));
        for (var i = 0; i < 5; i++)
        {
            await _client.GetAsync($"/?clientId={clientId}");
        }

        // Act
        var response = await _client.GetAsync($"/?clientId={otherClientId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}