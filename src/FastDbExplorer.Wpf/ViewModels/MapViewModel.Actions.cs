namespace FastDbExplorer.Wpf.ViewModels;

/// <summary>
/// A one-shot action that waits for the map page. Switching to the Map section builds a brand-new view and page, so a
/// command sent right away (for example "zoom to this layer" from the Ctrl+K palette) would be dropped: the page is not ready yet.
/// </summary>
public sealed partial class MapViewModel
{
    private Action? _afterPageReady;

    /// <summary>Runs <paramref name="action"/> once, right after the next time the map page reports "ready" (replaces an earlier waiting action).</summary>
    public void RunWhenPageReady(Action action) => _afterPageReady = action;

    /// <summary>Called by the view when the page is ready and the source and coordinate layers have been sent.</summary>
    public void OnPageReady()
    {
        var action = _afterPageReady;
        _afterPageReady = null;
        action?.Invoke();
    }
}
