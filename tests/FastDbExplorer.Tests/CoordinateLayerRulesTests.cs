using FastDbExplorer.Domain;
using Xunit;

namespace FastDbExplorer.Tests;

/// <summary>Invariants of coordinate layers (Domain rules). The layer model must never hold an invalid point.</summary>
public class CoordinateLayerRulesTests
{
    [Fact]
    public void A_valid_layer_keeps_its_points_name_and_defaults()
    {
        var id = Guid.NewGuid();
        var points = new[] { new MapPoint(Guid.NewGuid(), "Station A", 35.7219, 51.3347, id) };

        var layer = new MapLayer(id, "Stations", "stations.xlsx", points);

        Assert.Single(layer.Points);
        Assert.Equal("Stations", layer.Name);
        Assert.Equal("stations.xlsx", layer.FileName);
        Assert.True(layer.Visibility);
        Assert.Equal(MapLayer.DefaultRadiusMeters, layer.RadiusMeters);
    }

    [Fact]
    public void Rejects_an_empty_layer_id()
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new MapLayer(Guid.Empty, "Stations", "stations.xlsx", Array.Empty<MapPoint>()));
    }

    [Fact]
    public void Rejects_a_point_that_belongs_to_another_layer()
    {
        var id = Guid.NewGuid();
        var foreign = new[] { new MapPoint(Guid.NewGuid(), "A", 35.7, 51.3, Guid.NewGuid()) };

        Assert.ThrowsAny<ArgumentException>(() => new MapLayer(id, "Stations", "stations.xlsx", foreign));
    }

    [Fact]
    public void Rejects_a_point_without_a_name()
    {
        var id = Guid.NewGuid();
        var nameless = new[] { new MapPoint(Guid.NewGuid(), "   ", 35.7, 51.3, id) };

        Assert.ThrowsAny<ArgumentException>(() => new MapLayer(id, "Stations", "stations.xlsx", nameless));
    }

    [Theory]
    [InlineData(91.0, 51.3)]
    [InlineData(35.7, 181.0)]
    [InlineData(double.NaN, 51.3)]
    [InlineData(35.7, double.PositiveInfinity)]
    public void Rejects_coordinates_outside_wgs84_or_not_finite(double latitude, double longitude)
    {
        var id = Guid.NewGuid();
        var bad = new[] { new MapPoint(Guid.NewGuid(), "A", latitude, longitude, id) };

        Assert.ThrowsAny<ArgumentException>(() => new MapLayer(id, "Stations", "stations.xlsx", bad));
    }

    [Fact]
    public void Radius_setter_rejects_negative_values()
    {
        var id = Guid.NewGuid();
        var layer = new MapLayer(id, "Stations", "stations.xlsx", Array.Empty<MapPoint>());

        Assert.ThrowsAny<ArgumentOutOfRangeException>(() => layer.RadiusMeters = -1);
    }

    [Fact]
    public void Name_setter_rejects_blank_names()
    {
        var id = Guid.NewGuid();
        var layer = new MapLayer(id, "Stations", "stations.xlsx", Array.Empty<MapPoint>());

        Assert.ThrowsAny<ArgumentException>(() => layer.Name = "  ");
    }
}

/// <summary>Shared coordinate and radius limits (WGS84 and the largest meaningful radius).</summary>
public class CoordinateRulesTests
{
    [Theory]
    [InlineData(90.0, true)]
    [InlineData(-90.0, true)]
    [InlineData(90.0001, false)]
    [InlineData(double.NaN, false)]
    public void Latitude_range_is_wgs84(double value, bool expected)
        => Assert.Equal(expected, CoordinateRules.IsValidLatitude(value));

    [Theory]
    [InlineData(180.0, true)]
    [InlineData(-180.0, true)]
    [InlineData(-180.5, false)]
    [InlineData(double.PositiveInfinity, false)]
    public void Longitude_range_is_wgs84(double value, bool expected)
        => Assert.Equal(expected, CoordinateRules.IsValidLongitude(value));

    [Theory]
    [InlineData(0.0, true)]
    [InlineData(-1.0, false)]
    [InlineData(CoordinateRules.MaxRadiusMeters, true)]
    [InlineData(CoordinateRules.MaxRadiusMeters + 1, false)]
    public void Radius_range_is_metres_up_to_half_the_earth(double value, bool expected)
        => Assert.Equal(expected, CoordinateRules.IsValidRadiusMeters(value));
}
