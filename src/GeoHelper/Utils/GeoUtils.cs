﻿using GeoHelper.Models;
using H3;
using H3.Algorithms;
using H3.Extensions;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Polygon = GeoHelper.Models.Polygon;

namespace GeoHelper.Utils;

public static class GeoUtils
{
    private const int HexLimit = 100_000;
    
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
    /// <returns>A dictionary with the key being H3 hex id and the value being the boundary of the hex.</returns>
    /// <exception cref="ArgumentException">
    /// If too many hexes were found to lie within the provided polygon or collection. See <see cref="HexLimit"/>.
    /// </exception>
    public static Dictionary<string, string> GetH3HexesForPolygon(Polygon polygon, int resolution)
    {
        // Get the geometry object.
        var geo = new GeoJsonReader().Read<Geometry>(polygon.RawGeoJson);

        // Get the hexes that are within the geometry.
        var hexes = new List<H3Index>();
        switch (geo)
        {
            case MultiPolygon multi:
                HandleGeometryCollection(multi, resolution, hexes);
                break;
            case GeometryCollection collection:
                HandleGeometryCollection(collection, resolution, hexes);
                break;
            default:
                HandlePolygon(geo, resolution, hexes);
                break;
        }

        // Try not to kill the user's browser.
        if (hexes.Count > HexLimit)
        {
            throw new ArgumentException($"Polygon {polygon.Name} contains too many hexes" +
                                        $" at resolution {resolution}!");
        }
        
        // Parse the result into a dictionary.
        return hexes
            .ToDictionary(x => x.ToString(), y => new GeoJsonWriter().Write(y.GetCellBoundary())); 
    }

    /// <summary>
    /// Finds hexes for a single polygon.
    /// </summary>
    /// <param name="polygon">The polygon to find hexes for.</param>
    /// <param name="resolution">The H3 resolution to use.</param>
    /// <param name="hexes">Pass by reference. The output list.</param>
    private static void HandlePolygon(Geometry polygon, int resolution, List<H3Index> hexes)
    {
        hexes.AddRange(polygon.Fill(resolution));
    }
    
    /// <summary>
    /// Finds hexes for a collection of polygons. This works for both GEOMETRYCOLLECTION and MULTIPOLYGON.
    /// </summary>
    /// <param name="collection">The collection to find hexes for.</param>
    /// <param name="resolution">The H3 resolution to use.</param>
    /// <param name="hexes">Pass by reference. The output list.</param>
    private static void HandleGeometryCollection(GeometryCollection collection, int resolution, List<H3Index> hexes)
    {
        foreach (var geo in collection.Geometries)
        {
            if (geo is not null)
            {
                HandlePolygon(geo, resolution, hexes);
            }
        }
    }
    
    /// <summary>
    /// Converts a WKT (well known text) into a geoJson string.
    /// </summary>
    /// <param name="wkt">The WKT to convert.</param>
    /// <returns>A string representation of a geoJson object.</returns>
    /// <exception cref="ArgumentException">If the WKT could not be parse.</exception>
    private static string WktToGeoJsonString(string? wkt)
    {
        var geo = new WKTReader().Read(wkt)
                  ?? throw new ArgumentException("Could not parse WKT.");
        return new GeoJsonWriter().Write(geo);
    }
}