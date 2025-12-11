#region

using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

#endregion

namespace BeerService.Test;

[TestSubject(typeof(BeerApi))]
public class BeerApiTest : IClassFixture<WebApplicationFactory<Program>>
{
    public BeerApiTest(WebApplicationFactory<Program> factory)
    {
        _mockedService = new Mock<IBeerService>();
        var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services => { services.AddSingleton(_mockedService.Object); });
        }).CreateClient();
        _client = client;
    }

    private readonly Mock<IBeerService> _mockedService;
    private readonly HttpClient _client;

    [Fact]
    public async Task Should_get_beers()
    {
        List<BeerEntity> beers =
        [
            new() { Id = 0815, Brand = "Nestle", Name = "Wasser", Strength = 0.0 },
            new() { Id = 4711, Brand = "Schanzenbräu", Name = "Schanze Rot", Strength = 5.0 },
        ];
        _mockedService.Setup(x => x.FindAll()).Returns(beers);

        var response = await _client.GetAsync("/beers");

        response.EnsureSuccessStatusCode();
        var actual = await response.Content.ReadAsStringAsync();
        await VerifyJson(actual);
    }

    [Fact]
    public async Task Should_get_single_beer()
    {
        var beer = new BeerEntity
        {
            Id = 4711,
            Brand = "Schanzenbräu",
            Name = "Schanze Rot",
            Strength = 5.0
        };
        _mockedService.Setup(x => x.FindById(beer.Id)).ReturnsAsync(beer);

        var response = await _client.GetAsync("/beers/4711");

        response.EnsureSuccessStatusCode();
        var actual = await response.Content.ReadAsStringAsync();
        await VerifyJson(actual);
    }

    [Fact]
    public async Task Should_return_no_content_for_missing_beer()
    {
        var id = 4711;
        _mockedService.Setup(x => x.FindById(id)).ReturnsAsync((BeerEntity?)null);

        var response = await _client.GetAsync($"/beers/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var actual = await response.Content.ReadAsStringAsync();
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
            { Id = 4711, Brand = toCreate.Brand, Name = toCreate.Name, Strength = toCreate.Strength };
        _mockedService.Setup(x => x.Create(toCreate)).ReturnsAsync(created);

        var response = await _client.PostAsync(
            "/beers",
            new StringContent(json, new MediaTypeHeaderValue("application/json"))
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().Be($"/beers/{created.Id}");
        var actual = await response.Content.ReadAsStringAsync();
        await VerifyJson(actual);
    }

    [Fact]
    public async Task Should_delete_beer()
    {
        var id = 4711;

        var response = await _client.DeleteAsync($"/beers/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var actual = await response.Content.ReadAsStringAsync();
        actual.Should().BeEmpty();
        _mockedService.Verify(x => x.Delete(id), Times.Once);
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
            { Id = id, Brand = toUpdate.Brand, Name = toUpdate.Name, Strength = toUpdate.Strength };
        _mockedService.Setup(x => x.Update(id, toUpdate)).ReturnsAsync(created);

        var response = await _client.PutAsync(
            $"/beers/{id}",
            new StringContent(json, new MediaTypeHeaderValue("application/json"))
        );

        response.EnsureSuccessStatusCode();
        var actual = await response.Content.ReadAsStringAsync();
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
        _mockedService.Setup(x => x.Update(id, It.IsAny<BeerPayload>())).ReturnsAsync((BeerEntity?)null);

        var response = await _client.PutAsync(
            $"/beers/{id}",
            new StringContent(json, new MediaTypeHeaderValue("application/json"))
        );

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var actual = await response.Content.ReadAsStringAsync();
        actual.Should().BeEmpty();
    }
}