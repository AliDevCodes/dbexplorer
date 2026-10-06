using FastDbExplorer.Domain;
using FastDbExplorer.Infrastructure;
using Microsoft.Data.SqlClient;

namespace FastDbExplorer.Tests;

public class ReadOnlySqlGuardTests
{
    [Theory]
    [InlineData("SELECT name FROM sys.databases")]
    [InlineData("  select 1;  ")]
    [InlineData("WITH x AS (SELECT 1 AS a) SELECT a FROM x")]
    public void Allows_read_statements(string sql) => ReadOnlySqlGuard.EnsureReadOnly(sql);

    [Theory]
    [InlineData("")]
    [InlineData("DELETE FROM Orders")]
    [InlineData("UPDATE T SET a = 1")]
    [InlineData("INSERT INTO T VALUES (1)")]
    [InlineData("DROP TABLE T")]
    [InlineData("EXEC sp_who")]
    [InlineData("SELECT 1; DROP TABLE T")]
    [InlineData("SELECT * INTO Copy FROM T")]
    public void Rejects_everything_else(string sql)
        => Assert.Throws<InvalidOperationException>(() => ReadOnlySqlGuard.EnsureReadOnly(sql));
}

public class SqlConnectionStringFactoryTests
{
    [Fact]
    public void Windows_auth_is_read_only_intent()
    {
        var s = new ConnectionSettings(@".\SQLEXPRESS", AuthenticationMode.Windows, null, null, true);
        var b = new SqlConnectionStringBuilder(SqlConnectionStringFactory.Create(s));

        Assert.True(b.IntegratedSecurity);
        Assert.Equal(ApplicationIntent.ReadOnly, b.ApplicationIntent);
        Assert.Equal("master", b.InitialCatalog);
    }

    [Fact]
    public void Sql_auth_uses_credentials_and_selected_database()
    {
        var s = new ConnectionSettings("srv", AuthenticationMode.SqlServer, "reader", "pw", false);
        var b = new SqlConnectionStringBuilder(SqlConnectionStringFactory.Create(s, "Sales"));

        Assert.Equal("reader", b.UserID);
        Assert.Equal("Sales", b.InitialCatalog);
        Assert.False(b.TrustServerCertificate);
    }

    [Fact]
    public void Empty_server_is_rejected()
        => Assert.Throws<ArgumentException>(() =>
            SqlConnectionStringFactory.Create(new ConnectionSettings(" ", AuthenticationMode.Windows, null, null, true)));
}

public class JsonConnectionProfileStoreTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "fde-tests-" + Guid.NewGuid());
    private string FilePath => Path.Combine(_dir, "connections.json");

    [Fact]
    public async Task Saves_without_duplicates_and_never_writes_a_password()
    {
        var store = new JsonConnectionProfileStore(FilePath);
        var c = new SavedConnection("SRV1", AuthenticationMode.SqlServer, "reader");

        await store.SaveAsync(c);
        await store.SaveAsync(c with { Server = "srv1" });

        Assert.Single(await store.LoadAsync());
        Assert.DoesNotContain("assword", await File.ReadAllTextAsync(FilePath));
    }

    [Fact]
    public async Task Corrupt_file_loads_as_empty()
    {
        Directory.CreateDirectory(_dir);
        await File.WriteAllTextAsync(FilePath, "{ not json");
        Assert.Empty(await new JsonConnectionProfileStore(FilePath).LoadAsync());
    }

    public void Dispose() { if (Directory.Exists(_dir)) Directory.Delete(_dir, true); }
}
