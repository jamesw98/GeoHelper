using System.Text.Json.Serialization;

namespace GeoHelper.Models;

public class LeafletViewport
{
    [JsonPropertyName("_southWest")]
    public required SouthWest SouthWest { get; set; }

    [JsonPropertyName("_northEast")]
    public required NorthEast NorthEast { get; set; }

    public override string ToString()
    {
        return $"NE: {NorthEast.Lat}, {NorthEast.Lng}\nSW: {SouthWest.Lat}, {SouthWest.Lng}";
    }
}

public class NorthEast
{
    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lng")]
    public double Lng { get; set; }
}

public class SouthWest
{
    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lng")]
    public double Lng { get; set; }
}