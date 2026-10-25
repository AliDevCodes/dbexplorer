using FastDbExplorer.Domain;
using Xunit;

namespace FastDbExplorer.Tests;

/// <summary>Geodesic geometry used for radius circles and layer bounds (ADR-0006). Pure logic, no I/O.</summary>
public class GeoMathTests
{
    private const double TehranLat = 35.7219;
    private const double TehranLng = 51.3347;

    /// <summary>Great-circle distance with the same sphere as GeoMath (independent re-implementation for the check).</summary>
    private static double Haversine(double lat1, double lng1, double lat2, double lng2)
    {
        const double rad = Math.PI / 180;
        var dLat = (lat2 - lat1) * rad;
        var dLng = (lng2 - lng1) * rad;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(lat1 * rad) * Math.Cos(lat2 * rad) * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        return 2 * GeoMath.EarthRadiusMeters * Math.Asin(Math.Min(1, Math.Sqrt(a)));
    }

    [Fact]
    public void Circle_ring_is_closed_and_has_one_vertex_per_segment_plus_the_closing_point()
    {
        var ring = GeoMath.CircleRing(TehranLat, TehranLng, 500);

        Assert.Equal(GeoMath.CircleSegments + 1, ring.Count);
        Assert.Equal(ring[0], ring[^1]);
    }

    [Theory]
    [InlineData(35.7219, 51.3347, 500.0)]
    [InlineData(35.7219, 51.3347, 20000.0)]
    [InlineData(60.0, 25.0, 1000.0)]
    public void Every_vertex_is_exactly_the_radius_away_from_the_centre(double lat, double lng, double radius)
    {
        var ring = GeoMath.CircleRing(lat, lng, radius);

        foreach (var (vertexLng, vertexLat) in ring.Take(GeoMath.CircleSegments))
        {
            var distance = Haversine(lat, lng, vertexLat, vertexLng);
            Assert.InRange(distance, radius - 0.5, radius + 0.5);
        }
    }

    [Fact]
    public void Circle_ring_is_counter_clockwise_as_rfc_7946_requires()
    {
        var ring = GeoMath.CircleRing(TehranLat, TehranLng, 500);

        // Shoelace formula in lng/lat space: positive area means counter-clockwise.
        double twiceArea = 0;
        for (var i = 0; i < ring.Count - 1; i++)
            twiceArea += ring[i].Longitude * ring[i + 1].Latitude - ring[i + 1].Longitude * ring[i].Latitude;

        Assert.True(twiceArea > 0);
    }

    [Fact]
    public void Circle_over_the_antimeridian_is_not_wrapped()
    {
        var ring = GeoMath.CircleRing(0.0, 179.9999, 100_000);

        Assert.True(ring.Max(v => v.Longitude) > 180);
    }

    [Fact]
    public void Circle_rejects_latitude_outside_wgs84()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GeoMath.CircleRing(91, TehranLng, 500));
    }

    [Fact]
    public void Circle_rejects_longitude_outside_wgs84()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GeoMath.CircleRing(TehranLat, 181, 500));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    public void Circle_rejects_radius_that_is_not_positive(double radius)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GeoMath.CircleRing(TehranLat, TehranLng, radius));
    }

    [Fact]
    public void Circle_rejects_too_few_segments()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GeoMath.CircleRing(TehranLat, TehranLng, 500, segments: 7));
    }

    [Fact]
    public void Layer_bounds_are_null_without_points()
    {
        Assert.Null(GeoMath.LayerBounds(Array.Empty<MapPoint>(), 0));
    }

    [Fact]
    public void A_single_point_without_radius_gives_a_point_box()
    {
        var layerId = Guid.NewGuid();
        var points = new[] { new MapPoint(Guid.NewGuid(), "A", TehranLat, TehranLng, layerId) };

        var bounds = GeoMath.LayerBounds(points, 0);

        Assert.NotNull(bounds);
        Assert.True(bounds!.Value.IsPoint);
    }

    [Fact]
    public void A_radius_grows_the_bounds_on_all_sides()
    {
        var layerId = Guid.NewGuid();
        var points = new[] { new MapPoint(Guid.NewGuid(), "A", TehranLat, TehranLng, layerId) };

        var plain = GeoMath.LayerBounds(points, 0)!.Value;
        var grown = GeoMath.LayerBounds(points, 1000)!.Value;

        Assert.True(grown.West < plain.West);
        Assert.True(grown.East > plain.East);
        Assert.True(grown.South < plain.South);
        Assert.True(grown.North > plain.North);
    }
}

/// <summary>The radius the user types is metres only; the parser must never guess the unit (see ADR-0006).</summary>
public class RadiusInputTests
{
    [Theory]
    [InlineData("500", 500.0)]
    [InlineData("1.5", 1.5)]
    [InlineData(" 20 ", 20.0)]
    [InlineData("۱۵۰۰", 1500.0)]
    [InlineData("۱٫۵", 1.5)]
    public void Accepts_plain_and_persian_numbers(string text, double expected)
    {
        Assert.True(RadiusInput.TryParse(text, out var meters));
        Assert.Equal(expected, meters, 6);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1,500")]
    [InlineData("-5")]
    [InlineData("abc")]
    [InlineData("1e400")]
    public void Rejects_empty_ambiguous_negative_or_non_numeric_text(string? text)
    {
        Assert.False(RadiusInput.TryParse(text, out _));
    }
}
