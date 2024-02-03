using GeoHelper.Models;
using Microsoft.AspNetCore.Components.Forms;
using NetTopologySuite.IO.Esri;
using NetTopologySuite.IO.Esri.Dbf;
using NetTopologySuite.IO.Esri.Shapefiles.Readers;

namespace GeoHelper.Services;

public class ShapefileService
{
    /// <summary>
    /// Intentionally empty, for now.
    /// </summary>
    public ShapefileService()
    {
    }

    // public async Task<string[]> ReadDbfFile(IBrowserFile file)
    // {
    //     if (file.Name.Split('.').Last().ToLower() != "dbf")
    //     {
    //         throw new ArgumentException("Unknown file type.");
    //     }
    //     
    //     using var dbfReader = new DbfReader(file.OpenReadStream());
    //     await dbfReader.Initialize();
    //     
    //     return dbfReader.FirstOrDefault()?.GetNames()
    //            ?? throw new ArgumentException("Empty dbf file.");
    // }
    //
    // public List<Polygon> ReadShapefile(IBrowserFile file)
    // {
    //     // Shapefile.OpenRead()
    //     // var shapeReader = new ShapefilePolygonReader(file.OpenReadStream(), file.OpenReadStream());
    //     //
    //     // using var geos = Shapefile.ReadAllGeometries();
    //     return new List<Polygon>();
    // }
}