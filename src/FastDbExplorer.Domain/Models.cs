using System.Text.Json.Serialization;

namespace FastDbExplorer.Domain;

public enum AuthenticationMode { Windows, SqlServer }

/// <summary>Everything needed to open a connection. The password lives in memory only.</summary>
public sealed record ConnectionSettings(
    string Server,
    AuthenticationMode Authentication,
    string? UserName,
    string? Password,
    bool TrustServerCertificate);

/// <summary>A remembered connection. It deliberately has no password field.</summary>
public sealed record SavedConnection(string Server, AuthenticationMode Authentication, string? UserName)
{
    [JsonIgnore]
    public string Description => Authentication == AuthenticationMode.Windows ? "Windows" : $"SQL · {UserName}";
}

public sealed record DatabaseInfo(string Name);

public sealed record TableInfo(string Schema, string Name, long ApproxRows)
{
    public string FullName => $"{Schema}.{Name}";
}

public enum DatabaseErrorKind { LoginFailed, ServerUnreachable, Timeout, Other }

public sealed class DatabaseAccessException(DatabaseErrorKind kind, string message, Exception? inner = null)
    : Exception(message, inner)
{
    public DatabaseErrorKind Kind { get; } = kind;
}
