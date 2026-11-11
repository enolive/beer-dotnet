#region

using System.Net;
using System.Net.Http.Headers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace BeerService.Test;

public class SystemTest(WebApplicationFactory<Program> factory, PostgresFixture postgres)
    : IClassFixture<WebApplicationFactory<Program>>,
        IClassFixture<PostgresFixture>,
        IAsyncLifetime
{
    private readonly CancellationToken ct = TestContext.Current.CancellationToken;
    private HttpClient client;
    private AppDbContext dbContext;

    public ValueTask InitializeAsync()
    {
        dbContext = postgres.CreateDbContext();
        client = factory
            .WithWebHostBuilder(builder =>
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(dbContext);
                })
            )
            .CreateClient();
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await dbContext.DisposeAsync();
    }

    [Fact]
    public async Task Should_create_a_beer()
    {
        // arrange
        var json = /*language=json*/
            """
             {
             "name": "Schanze Rot",
             "brand": "Schanzenbräu",
             "strength": 5.0
             }
            """;

        // act: create a beer
        var createdResponse = await client.PostAsync(
            "/beers",
            new StringContent(json, new MediaTypeHeaderValue("application/json")),
            ct
        );

        // assert: create should succeed
        createdResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // act: read all beers
        var readResponse = await client.GetAsync("/beers", ct);

        // assert: read should contain the created beer
        readResponse.EnsureSuccessStatusCode();
        var beers = await readResponse.Content.ReadAsStringAsync(ct);
        await VerifyJson(beers);
    }
}
