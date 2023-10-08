namespace GeoHelper.Models;

public class Polygon
{
    public string? Name { get; set; }
    public string? RawInput { get; set; }
    public string? RawGeoJson { get; set; }
    public string? HexColor { get; set; }
    public PolygonTypes Type { get; set; }
}