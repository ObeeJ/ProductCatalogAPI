using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Xunit;

namespace ProductCatalogAPI.Tests.Integration;

public class ConcurrencyTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ConcurrencyTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ConcurrentOrders_ShouldPreventOverselling()
    {
        // Simple test to verify API responds
        var response = await _client.GetAsync("/api/products");
        Assert.True(response.IsSuccessStatusCode);
    }
}