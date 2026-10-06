using System.Text.Json.Nodes;
using FastDbExplorer.Domain;

namespace FastDbExplorer.Application.CoordinateLayers;

/// <summary>
/// Turns a <see cref="MapLayer"/> into the GeoJSON the map page draws. Pure data, no UI types, so it is unit-tested.
/// Coordinates are WGS84 [longitude, latitude] (GeoJSON order). Radius circles are real polygons built in
/// <see cref="GeoMath.CircleRing"/>; the page never converts metres itself.
/// </summary>
public static class CoordinateLayerGeoJson
{
    /// <summary>One Point feature per layer point, with its name (for the label).</summary>
    public static JsonObject Points(MapLayer layer)
    {
        ArgumentNullException.ThrowIfNull(layer);
        var features = new JsonArray();
        foreach (var point in layer.Points)
        {
            features.Add(new JsonObject
            {
                ["type"] = "Feature",
                ["geometry"] = new JsonObject
                {
                    ["type"] = "Point",
                    ["coordinates"] = new JsonArray(point.Longitude, point.Latitude)
                },
                ["properties"] = new JsonObject { ["id"] = point.Id.ToString(), ["name"] = point.Name }
            });
        }
        return Collection(features);
    }

    /// <summary>One Polygon feature per point when the layer radius is above 0; an empty collection otherwise.</summary>
    public static JsonObject Circles(MapLayer layer)
    {
        ArgumentNullException.ThrowIfNull(layer);
        var features = new JsonArray();
        if (layer.RadiusMeters > 0)
        {
            foreach (var point in layer.Points)
            {
                var ring = new JsonArray();
                foreach (var (lng, lat) in GeoMath.CircleRing(point.Latitude, point.Longitude, layer.RadiusMeters))
                    ring.Add(new JsonArray(lng, lat));

                features.Add(new JsonObject
                {
                    ["type"] = "Feature",
                    ["geometry"] = new JsonObject
                    {
                        ["type"] = "Polygon",
                        ["coordinates"] = new JsonArray(ring)
                    },
                    ["properties"] = new JsonObject { ["id"] = point.Id.ToString(), ["name"] = point.Name }
                });
            }
        }
        return Collection(features);
    }

    /// <summary>[west, south, east, north] around all points and their circles; null for a layer without points.</summary>
    public static JsonArray? Bounds(MapLayer layer)
    {
        ArgumentNullException.ThrowIfNull(layer);
        return GeoMath.LayerBounds(layer.Points, layer.RadiusMeters) is { } b
            ? new JsonArray(b.West, b.South, b.East, b.North)
            : null;
    }

    private static JsonObject Collection(JsonArray features) =>
        new() { ["type"] = "FeatureCollection", ["features"] = features };
}
