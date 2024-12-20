using GeoHelper.Models;
using H3;
using H3.Algorithms;
using H3.Extensions;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Polygon = GeoHelper.Models.Polygon;

namespace GeoHelper.Utils;

public static class GeoUtils
{
    private const int HexLimit = 100_000;
    private static readonly GeoJsonReader Reader = new GeoJsonReader();

    /// <summary>
    /// Converts a geojson string into a Geometry object.
    /// </summary>
    /// <param name="geoJsonString"></param>
    /// <returns></returns>
    public static Geometry GeoJsonStringToPolygon(string geoJsonString)
    {
        return new GeoJsonReader().Read<IFeature>(geoJsonString).Geometry;
    }

    /// <summary>
    /// Prepares a polygon to be added to the map.
    /// </summary>
    /// <param name="polygon">The polygon to prepare.</param>
    /// <exception cref="ArgumentException">If something went wrong during preparation.</exception>
    public static void PreparePolygon(Polygon polygon)
    {
        // Ensure the user actually entered something.
        if (string.IsNullOrWhiteSpace(polygon.RawInput))
        {
            throw new ArgumentException("Please input a polygon.");
        }

        try
        {
            // Get the geoJson for this polygon.
            polygon.RawGeoJson = polygon.Type == PolygonTypes.GeoJson
                ? polygon.RawInput
                : WktToGeoJsonString(polygon.RawInput);
        }
        catch (Exception e) when (e is ArgumentException or ParseException)
        {
            throw new ArgumentException("Failed to parse geometry.");
        }
        catch (Exception)
        {
            throw new ArgumentException("An unexpected error occured. Please check your settings.");
        }
    }

    /// <summary>
    /// Finds H3 hexes that fall within a given polygon or collection of polygons
    /// </summary>
    /// <param name="polygon">The polygon or collection to find hexes for.</param>
    /// <param name="resolution">The resolution of H3 hexes to use.</param>
    /// <param name="bounds">
    /// The viewport to get hexes within. This makes it so we can actually get higher resolution hexes for larger
    /// polygons without killing the browser
    /// </param>
    /// <returns>A dictionary with the key being H3 hex id and the value being the boundary of the hex.</returns>
    /// <exception cref="ArgumentException">
    /// If too many hexes were found to lie within the provided polygon or collection. See <see cref="HexLimit"/>.
    /// </exception>
    public static Dictionary<string, string> GetH3HexesForPolygon(Polygon polygon, int resolution, LeafletViewport bounds)
    {
        // If the polygon is from leaflet, we need to try getting a feature, if not, just get a normal geometry.
        var geo = polygon.Type == PolygonTypes.FromLeaflet 
            ? Reader.Read<IFeature>(polygon.RawGeoJson).Geometry 
            : Reader.Read<Geometry>(polygon.RawGeoJson);

        // Get the bounding box. 
        var boundingBox = new GeometryFactory().CreatePolygon([
            new Coordinate(bounds.SouthWest.Lng, bounds.SouthWest.Lat), 
            new Coordinate(bounds.NorthEast.Lng, bounds.SouthWest.Lat), 
            new Coordinate(bounds.NorthEast.Lng, bounds.NorthEast.Lat), 
            new Coordinate(bounds.SouthWest.Lng, bounds.NorthEast.Lat), 
            new Coordinate(bounds.SouthWest.Lng, bounds.SouthWest.Lat)
        ]) ?? throw new ArgumentException($"Could not create bounding box for bounds {bounds}");

        // Get the hexes that are within the geometry.
        var hexes = new List<H3Index>();
        switch (geo)
        {
            case GeometryCollection collection:
                HandleGeometryCollection(collection, resolution, hexes, boundingBox);
                break;
            default:
                HandlePolygon(geo, resolution, hexes, boundingBox);
                break;
        }

        // Try not to kill the user's browser.
        if (hexes.Count > HexLimit)
        {
            throw new ArgumentException($"Polygon {polygon.Name} contains too many hexes at resolution {resolution}!");
        }

        // Parse the result into a dictionary.
        return hexes.ToDictionary(x => x.ToString(), y => new GeoJsonWriter().Write(y.GetCellBoundary()));
    }

    /// <summary>
    /// Finds hexes for a single polygon.
    /// </summary>
    /// <param name="polygon">The polygon to find hexes for.</param>
    /// <param name="resolution">The H3 resolution to use.</param>
    /// <param name="hexes">Pass by reference. The output list.</param>
    /// <param name="boundingBox">The viewport to get hexes within.</param>
    private static void HandlePolygon(Geometry polygon, int resolution, List<H3Index> hexes, Geometry boundingBox)
    {

        var intersectPoly = boundingBox.Intersection(polygon);
        if (intersectPoly is GeometryCollection geoColl)
        {
            foreach (var g in geoColl)
            {
                if (g is null)
                {
                    throw new ArgumentException("Found null geometry when attempting to get hexes.");
                }
                hexes.AddRange(g.Fill(resolution));
            }
        }
        else
        {
            // Get just the hexes that are both within the bounding box and within the requested polygon.
            hexes.AddRange(intersectPoly.Fill(resolution));
        }
        
    }

    /// <summary>
    /// Finds hexes for a collection of polygons. This works for both GEOMETRYCOLLECTION and MULTIPOLYGON.
    /// </summary>
    /// <param name="collection">The collection to find hexes for.</param>
    /// <param name="resolution">The H3 resolution to use.</param>
    /// <param name="hexes">Pass by reference. The output list.</param>
    /// <param name="boundingBox">The viewport to get hexes within.</param>
    private static void HandleGeometryCollection(GeometryCollection collection, int resolution, List<H3Index> hexes, Geometry boundingBox)
    {
        foreach (var geo in collection.Geometries)
        {
            if (geo is not null)
            {
                HandlePolygon(geo, resolution, hexes, boundingBox);
            }
        }
    }

    /// <summary>
    /// Converts a WKT (well known text) into a geoJson string.
    /// </summary>
    /// <param name="wkt">The WKT to convert.</param>
    /// <returns>A string representation of a geoJson object.</returns>
    /// <exception cref="ArgumentException">If the WKT could not be parsed.</exception>
    private static string WktToGeoJsonString(string? wkt)
    {
        var geo = new WKTReader().Read(wkt)
                  ?? throw new ArgumentException("Could not parse WKT.");
        return new GeoJsonWriter().Write(geo);
    }
}