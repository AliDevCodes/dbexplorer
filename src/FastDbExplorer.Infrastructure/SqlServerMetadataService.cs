using System.Data;
using FastDbExplorer.Application.Abstractions;
using FastDbExplorer.Domain;
using Microsoft.Data.SqlClient;

namespace FastDbExplorer.Infrastructure;

public sealed class SqlServerMetadataService : IDatabaseMetadataService
{
    private const int CommandTimeoutSeconds = 30;

    // System databases (ids 1-4) are hidden: this tool is for user data.
    private const string DatabasesSql =
        "SELECT name FROM sys.databases WHERE database_id > 4 AND state = 0 AND HAS_DBACCESS(name) = 1 ORDER BY name";

    // sys.partitions is cheap metadata (no table scan), so row counts are approximate but instant.
    private const string TablesSql = """
        SELECT s.name, t.name, SUM(p.rows)
        FROM sys.tables AS t
        JOIN sys.schemas AS s ON s.schema_id = t.schema_id
        JOIN sys.partitions AS p ON p.object_id = t.object_id AND p.index_id IN (0, 1)
        GROUP BY s.name, t.name
        ORDER BY s.name, t.name
        """;

    private const string ColumnsSql = """
        SELECT c.name, TYPE_NAME(c.system_type_id), ISNULL(k.key_ordinal, 0)
        FROM sys.columns AS c
        LEFT JOIN (
            SELECT ic.object_id, ic.column_id, ic.key_ordinal
            FROM sys.indexes AS i
            JOIN sys.index_columns AS ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
            WHERE i.is_primary_key = 1) AS k ON k.object_id = c.object_id AND k.column_id = c.column_id
        WHERE c.object_id = OBJECT_ID(@fullName)
        ORDER BY c.column_id
        """;

    public async Task TestConnectionAsync(ConnectionSettings settings, CancellationToken ct)
        => await QueryAsync(settings, null, "SELECT 1", null, r => r.GetInt32(0), ct);

    public async Task<IReadOnlyList<DatabaseInfo>> GetDatabasesAsync(ConnectionSettings settings, CancellationToken ct)
        => await QueryAsync(settings, null, DatabasesSql, null, r => new DatabaseInfo(r.GetString(0)), ct);

    public async Task<IReadOnlyList<TableInfo>> GetTablesAsync(ConnectionSettings settings, string database, CancellationToken ct)
        => await QueryAsync(settings, database, TablesSql, null,
            r => new TableInfo(r.GetString(0), r.GetString(1), r.GetInt64(2)), ct);

    public async Task<IReadOnlyList<ColumnInfo>> GetColumnsAsync(
        ConnectionSettings settings, string database, string schema, string table, CancellationToken ct)
    {
        var fullName = SelectQueryBuilder.Quote(schema) + "." + SelectQueryBuilder.Quote(table);
        QueryParameter[] parameters = [new("@fullName", SqlDbType.NVarChar, fullName)];
        return await QueryAsync(settings, database, ColumnsSql, parameters,
            r => new ColumnInfo(r.GetString(0), r.GetString(1), Convert.ToInt32(r.GetValue(2))), ct);
    }

    public async Task<PageResult> GetPageAsync(ConnectionSettings settings, PageRequest request, CancellationToken ct)
    {
        var columns = await GetColumnsAsync(settings, request.Database, request.Schema, request.Table, ct);
        var query = SelectQueryBuilder.Build(request, columns);
        var pageSize = Math.Clamp(request.PageSize, 1, SelectQueryBuilder.MaxPageSize);
        var keyCount = request.UseKeyset ? request.OrderBy.Count : 0;
        var valueCount = request.Columns.Count;

        return await ExecuteAsync(settings, request.Database, query.Sql, query.Parameters, async (reader, token) =>
        {
            var rows = new List<object?[]>();
            object?[]? lastKey = null;
            var hasMore = false;

            while (await reader.ReadAsync(token))
            {
                if (rows.Count == pageSize) { hasMore = true; break; } // the +1 row only proves a next page exists

                var values = new object?[valueCount];
                for (var i = 0; i < valueCount; i++)
                {
                    var v = reader.GetValue(i);
                    values[i] = v is DBNull ? null : v;
                }
                rows.Add(values);

                if (keyCount > 0)
                {
                    var key = new object?[keyCount];
                    for (var k = 0; k < keyCount; k++) key[k] = reader.GetValue(valueCount + k);
                    lastKey = key;
                }
            }
            return new PageResult(request.Columns, rows, hasMore, lastKey);
        }, ct);
    }

    private static Task<List<T>> QueryAsync<T>(
        ConnectionSettings settings, string? database, string sql, IReadOnlyList<QueryParameter>? parameters,
        Func<SqlDataReader, T> map, CancellationToken ct)
        => ExecuteAsync(settings, database, sql, parameters, async (reader, token) =>
        {
            var result = new List<T>();
            while (await reader.ReadAsync(token))
                result.Add(map(reader));
            return result;
        }, ct);

    private static async Task<T> ExecuteAsync<T>(
        ConnectionSettings settings, string? database, string sql, IReadOnlyList<QueryParameter>? parameters,
        Func<SqlDataReader, CancellationToken, Task<T>> read, CancellationToken ct)
    {
        ReadOnlySqlGuard.EnsureReadOnly(sql);
        try
        {
            await using var connection = new SqlConnection(SqlConnectionStringFactory.Create(settings, database));
            await connection.OpenAsync(ct);

            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = CommandTimeoutSeconds;
            if (parameters is not null)
                foreach (var p in parameters) command.Parameters.Add(ToSqlParameter(p));

            // SequentialAccess streams rows; readers must read columns in order.
            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct);
            return await read(reader, ct);
        }
        catch (SqlException) when (ct.IsCancellationRequested)
        {
            throw new OperationCanceledException(ct);
        }
        catch (SqlException ex)
        {
            throw new DatabaseAccessException(Classify(ex), ex.Message, ex);
        }
    }

    private static SqlParameter ToSqlParameter(QueryParameter p)
    {
        var parameter = new SqlParameter(p.Name, p.Type) { Value = p.Value };
        var length = (p.Value as string)?.Length ?? 0;
        if (p.Type == SqlDbType.NVarChar) parameter.Size = length <= 4000 ? 4000 : -1; // fixed sizes = fewer cached plans
        if (p.Type == SqlDbType.VarChar) parameter.Size = length <= 8000 ? 8000 : -1;
        if (p.Type == SqlDbType.Decimal && p.Value is decimal d)
        {
            parameter.Precision = 38;
            parameter.Scale = (byte)((decimal.GetBits(d)[3] >> 16) & 0xFF);
        }
        return parameter;
    }

    private static DatabaseErrorKind Classify(SqlException ex) => ex.Number switch
    {
        18456 or 4060 or 18452 => DatabaseErrorKind.LoginFailed,
        -2 => DatabaseErrorKind.Timeout,
        -1 or 2 or 40 or 53 or 258 or 10060 or 10061 => DatabaseErrorKind.ServerUnreachable,
        _ => DatabaseErrorKind.Other
    };
}
