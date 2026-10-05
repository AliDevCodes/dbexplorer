using System.Text.Json;
using System.Text.Json.Serialization;
using FastDbExplorer.Application.Abstractions;
using FastDbExplorer.Domain;

namespace FastDbExplorer.Infrastructure;

/// <summary>Stores recent connections (never passwords) in %AppData%\FastDbExplorer\connections.json.</summary>
public sealed class JsonConnectionProfileStore : IConnectionProfileStore
{
    private const int MaxItems = 8;
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _path;

    public JsonConnectionProfileStore(string? path = null)
        => _path = path ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FastDbExplorer", "connections.json");

    public async Task<IReadOnlyList<SavedConnection>> LoadAsync(CancellationToken ct = default)
    {
        if (!File.Exists(_path)) return [];
        try
        {
            await using var stream = File.OpenRead(_path);
            return await JsonSerializer.DeserializeAsync<List<SavedConnection>>(stream, Options, ct) ?? [];
        }
        catch (JsonException)
        {
            return []; // A corrupt history file must never block the app; it is rewritten on next save.
        }
    }

    public async Task SaveAsync(SavedConnection connection, CancellationToken ct = default)
    {
        var items = (await LoadAsync(ct)).Where(c => !IsSame(c, connection)).ToList();
        items.Insert(0, connection);
        await WriteAsync(items.Take(MaxItems).ToList(), ct);
    }

    public async Task RemoveAsync(SavedConnection connection, CancellationToken ct = default)
    {
        var items = (await LoadAsync(ct)).Where(c => !IsSame(c, connection)).ToList();
        await WriteAsync(items, ct);
    }

    private async Task WriteAsync(List<SavedConnection> items, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        await using var stream = File.Create(_path);
        await JsonSerializer.SerializeAsync(stream, items, Options, ct);
    }

    private static bool IsSame(SavedConnection a, SavedConnection b) =>
        string.Equals(a.Server, b.Server, StringComparison.OrdinalIgnoreCase)
        && a.Authentication == b.Authentication
        && string.Equals(a.UserName, b.UserName, StringComparison.OrdinalIgnoreCase);
}
