using System.Text.Json;
using FastDbExplorer.Domain;
using FastDbExplorer.Wpf.Localization;

namespace FastDbExplorer.Wpf.Views;

/// <summary>
/// Host side of the map tools overlay (map-tools.js). The page draws the tools itself; this part sends it the texts and the
/// state of the Explorer/Monitoring result layer, tells it which kind of layer is which (so each source gets its own marker
/// shape), and carries out the few things the page cannot do alone: save/delete a "my points" point, zoom to or remove the results.
/// </summary>
public partial class MapView
{
    private void PushToolsInit() => Post(new { type = "toolsInit", labels = MapToolStrings.Labels });

    /// <summary>Tells the page about the temporary Explorer/Monitoring layer (a chip with zoom and remove), or that there is none.</summary>
    private void PostToolResult()
    {
        if (_viewModel?.QueryResultLayer is { } layer)
        {
            Post(new
            {
                type = "toolResult",
                id = layer.Id.ToString(),
                title = layer.Name,
                count = layer.Points.Count,
                color = MapSourceStyles.QueryColor
            });
        }
        else
        {
            Post(new { type = "toolResult", id = (string?)null, title = "", count = 0, color = "" });
        }
    }

    private MapLayerKind KindOf(MapLayer layer)
    {
        if (layer.Id == MapViewModel.ManualLayerId) return MapLayerKind.Manual;
        return _viewModel?.QueryResultLayer?.Id == layer.Id ? MapLayerKind.Query : MapLayerKind.Excel;
    }

    /// <summary>Handles the messages of the tools overlay; false = not a tools message.</summary>
    private bool HandleToolMessage(string? type, JsonElement root)
    {
        if (_viewModel is null) return false;
        switch (type)
        {
            case "toolAddPoint":
                if (root.TryGetProperty("lat", out var lat) && lat.TryGetDouble(out var latitude)
                    && root.TryGetProperty("lng", out var lng) && lng.TryGetDouble(out var longitude))
                    _ = _viewModel.AddManualPointAsync(Text(root, "name"), latitude, longitude);
                return true;
            case "toolDeletePoint":
                if (Guid.TryParse(Text(root, "pointId"), out var pointId)) _ = _viewModel.RemoveManualPointAsync(pointId);
                return true;
            case "toolZoomResult":
                _viewModel.ZoomToQueryResult();
                return true;
            case "toolClearResult":
                _viewModel.ClearQueryResultLayer();
                return true;
            default:
                return false;
        }
    }
}
