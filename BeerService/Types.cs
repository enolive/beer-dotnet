#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace BeerService;

public record BeerPayload(string Name, string Brand, double Strength);

public class BeerEntity
{
    [Key] public int Id { get; set; }

    public required string Name { get; set; }

    public required string Brand { get; set; }

    public required double Strength { get; set; }
}