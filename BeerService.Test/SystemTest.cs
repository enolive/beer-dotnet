#region

using System.Net;
using System.Net.Http.Headers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace BeerService.Test;

public class SystemTest :
    IClassFixture<WebApplicationFactory<Program>>,
    IClassFixture<PostgresFixture>
{
    private readonly HttpClient _client;

    public SystemTest(WebApplicationFactory<Program> factory, PostgresFixture postgres)
    {
        _client = factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            services.AddSingleton(postgres.DbContext);
        })).CreateClient();
    }

    [Fact]
    public async Task Should_create_a_beer()
    {
        var json = /*language=json*/
            """
             {
             "name": "Schanze Rot",
             "brand": "Schanzenbräu",
             "strength": 5.0
             }
            """;

        var createdResponse = await _client.PostAsync(
            "/beers",
            new StringContent(json, new MediaTypeHeaderValue("application/json"))
        );

        createdResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var readResponse = await _client.GetAsync("/beers");

        readResponse.EnsureSuccessStatusCode();
        var beers = await readResponse.Content.ReadAsStringAsync();
        await Verify(beers);
    }
}