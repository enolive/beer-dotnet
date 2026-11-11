#region

using System.Net;
using System.Net.Http.Headers;
using AwesomeAssertions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

#endregion

namespace BeerService.Test;

[TestSubject(typeof(BeerApi))]
public class BeerApiTest : IAsyncLifetime
{
    private readonly CancellationToken ct = TestContext.Current.CancellationToken;
    private readonly Mock<IBeerService> mockedService = new();
    private WebApplication app = null!;
    private HttpClient client = null!;

    public async ValueTask InitializeAsync()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton(mockedService.Object);
        app = builder.Build();
        app.ConfigureBeerApiEndpoints();
        await app.StartAsync(ct);
        client = app.GetTestClient();
    }

    public async ValueTask DisposeAsync()
    {
        client.Dispose();
        await app.DisposeAsync();
    }

    [Fact]
    public async Task Should_get_beers()
    {
        List<BeerEntity> beers =
        [
            new()
            {
                Id = 0815,
                Details = new BeerDetails
                {
                    Brand = "Nestle",
                    Name = "Wasser",
                    Strength = 0.0,
                },
            },
            new()
            {
                Id = 4711,
                Details = new BeerDetails
                {
                    Brand = "Schanzenbräu",
                    Name = "Schanze Rot",
                    Strength = 5.0,
                },
            },
        ];
        mockedService.Setup(x => x.FindAll()).Returns(beers.ToAsyncEnumerable());

        var response = await client.GetAsync("/beers", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var actual = await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken
        );
        await VerifyJson(actual);
    }

    [Fact]
    public async Task Should_get_single_beer()
    {
        var beer = new BeerEntity
        {
            Id = 4711,
            Details = new BeerDetails
            {
                Brand = "Schanzenbräu",
                Name = "Schanze Rot",
                Strength = 5.0,
            },
        };
        mockedService
            .Setup(x => x.FindById(beer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(beer);

        var response = await client.GetAsync("/beers/4711", ct);

        response.EnsureSuccessStatusCode();
        var actual = await response.Content.ReadAsStringAsync(ct);
        await VerifyJson(actual);
    }

    [Fact]
    public async Task Should_return_no_content_for_missing_beer()
    {
        var id = 4711;
        mockedService
            .Setup(x => x.FindById(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BeerEntity?)null);

        var response = await client.GetAsync($"/beers/{id}", ct);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var actual = await response.Content.ReadAsStringAsync(ct);
        actual.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_create_beer()
    {
        var json = /*language=json*/
            """
             {
             "name": "Schanze Rot",
             "brand": "Schanzenbräu",
             "strength": 5.0
             }
            """;
        var toCreate = new BeerPayload("Schanze Rot", "Schanzenbräu", 5.0);
        var created = new BeerEntity
        {
            Id = 4711,
            Details = new BeerDetails
            {
                Brand = toCreate.Brand,
                Name = toCreate.Name,
                Strength = toCreate.Strength,
            },
        };
        mockedService
            .Setup(x => x.Create(toCreate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var response = await client.PostAsync(
            "/beers",
            new StringContent(json, new MediaTypeHeaderValue("application/json")),
            ct
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().Be($"/beers/{created.Id}");
        var actual = await response.Content.ReadAsStringAsync(ct);
        await VerifyJson(actual);
    }

    [Fact]
    public async Task Should_delete_beer()
    {
        var id = 4711;

        var response = await client.DeleteAsync($"/beers/{id}", ct);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var actual = await response.Content.ReadAsStringAsync(ct);
        actual.Should().BeEmpty();
        mockedService.Verify(x => x.Delete(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_update_existing_beer()
    {
        var id = 4711;
        var json = /*language=json*/
            """
             {
             "name": "Schanze Rot",
             "brand": "Schanzenbräu",
             "strength": 5.0
             }
            """;
        var toUpdate = new BeerPayload("Schanze Rot", "Schanzenbräu", 5.0);
        var created = new BeerEntity
        {
            Id = id,
            Details = new BeerDetails
            {
                Brand = toUpdate.Brand,
                Name = toUpdate.Name,
                Strength = toUpdate.Strength,
            },
        };
        mockedService
            .Setup(x => x.Update(id, toUpdate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var response = await client.PutAsync(
            $"/beers/{id}",
            new StringContent(json, new MediaTypeHeaderValue("application/json")),
            ct
        );

        response.EnsureSuccessStatusCode();
        var actual = await response.Content.ReadAsStringAsync(ct);
        await VerifyJson(actual);
    }

    [Fact]
    public async Task Should_return_no_content_for_missing_beer_on_update()
    {
        var id = 4711;
        var json = /*language=json*/
            """
             {
             "name": "Schanze Rot",
             "brand": "Schanzenbräu",
             "strength": 5.0
             }
            """;
        mockedService
            .Setup(x => x.Update(id, It.IsAny<BeerPayload>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BeerEntity?)null);

        var response = await client.PutAsync(
            $"/beers/{id}",
            new StringContent(json, new MediaTypeHeaderValue("application/json")),
            ct
        );

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var actual = await response.Content.ReadAsStringAsync(ct);
        actual.Should().BeEmpty();
    }
}
