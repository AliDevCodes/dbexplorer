using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;
using FastDbExplorer.Application.Abstractions;

namespace FastDbExplorer.Infrastructure.Monitoring;

/// <summary>
/// Writes a real .xlsx file (a small SpreadsheetML package) with no third-party library.
/// One sheet "Records": bold header row, frozen, right-to-left view. Text is stored as inline strings.
/// </summary>
public sealed class XlsxReportWriter : IReportWriter, IQueryReportWriter
{
    private const string Ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private const int MaxCellChars = 32_000;
    private const int MaxExcelRows = 1_048_576;
    private const long MaxExactInteger = 999_999_999_999_999; // Excel keeps 15 digits; longer integers are written as text
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    private static readonly UTF8Encoding Utf8 = new(false);

    private const string ContentTypes =
        "<?xml version='1.0' encoding='UTF-8' standalone='yes'?><Types xmlns='http://schemas.openxmlformats.org/package/2006/content-types'>"
        + "<Default Extension='rels' ContentType='application/vnd.openxmlformats-package.relationships+xml'/>"
        + "<Default Extension='xml' ContentType='application/xml'/>"
        + "<Override PartName='/xl/workbook.xml' ContentType='application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml'/>"
        + "<Override PartName='/xl/worksheets/sheet1.xml' ContentType='application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml'/>"
        + "<Override PartName='/xl/styles.xml' ContentType='application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml'/></Types>";

    private const string RootRels =
        "<?xml version='1.0' encoding='UTF-8' standalone='yes'?><Relationships xmlns='http://schemas.openxmlformats.org/package/2006/relationships'>"
        + "<Relationship Id='rId1' Type='http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument' Target='xl/workbook.xml'/></Relationships>";

    private const string Workbook =
        "<?xml version='1.0' encoding='UTF-8' standalone='yes'?><workbook xmlns='http://schemas.openxmlformats.org/spreadsheetml/2006/main' xmlns:r='http://schemas.openxmlformats.org/officeDocument/2006/relationships'>"
        + "<sheets><sheet name='Records' sheetId='1' r:id='rId1'/></sheets></workbook>";

    private const string WorkbookRels =
        "<?xml version='1.0' encoding='UTF-8' standalone='yes'?><Relationships xmlns='http://schemas.openxmlformats.org/package/2006/relationships'>"
        + "<Relationship Id='rId1' Type='http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet' Target='worksheets/sheet1.xml'/>"
        + "<Relationship Id='rId2' Type='http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles' Target='styles.xml'/></Relationships>";

    private const string Styles =
        "<?xml version='1.0' encoding='UTF-8' standalone='yes'?><styleSheet xmlns='http://schemas.openxmlformats.org/spreadsheetml/2006/main'>"
        + "<fonts count='2'><font><sz val='11'/><name val='Calibri'/></font><font><b/><sz val='11'/><name val='Calibri'/></font></fonts>"
        + "<fills count='2'><fill><patternFill patternType='none'/></fill><fill><patternFill patternType='gray125'/></fill></fills>"
        + "<borders count='1'><border><left/><right/><top/><bottom/><diagonal/></border></borders>"
        + "<cellStyleXfs count='1'><xf numFmtId='0' fontId='0' fillId='0' borderId='0'/></cellStyleXfs>"
        + "<cellXfs count='2'><xf numFmtId='0' fontId='0' fillId='0' borderId='0' xfId='0'/><xf numFmtId='0' fontId='1' fillId='0' borderId='0' xfId='0' applyFont='1'/></cellXfs>"
        + "<cellStyles count='1'><cellStyle name='Normal' xfId='0' builtinId='0'/></cellStyles></styleSheet>";

    public Task<string> WriteAsync(
        string monitorName, DateTime checkedAtUtc, IReadOnlyList<string> columns,
        IReadOnlyList<object?[]> rows, string folder, CancellationToken ct)
        => Task.Run(() => Write(monitorName, checkedAtUtc, columns, rows, folder, ct), ct);

    public Task WriteQueryAsync(
        IReadOnlyList<string> columns, IAsyncEnumerable<object?[]> rows, string path, CancellationToken ct)
        => Task.Run(() => WriteQueryCoreAsync(columns, rows, path, ct), ct);

    private static async Task WriteQueryCoreAsync(
        IReadOnlyList<string> columns, IAsyncEnumerable<object?[]> rows, string path, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var fullPath = Path.GetFullPath(path);
        var folder = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(folder);
        var temp = fullPath + ".tmp";
        try
        {
            using (var file = File.Create(temp))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Create))
            {
                AddText(zip, "[Content_Types].xml", ContentTypes);
                AddText(zip, "_rels/.rels", RootRels);
                AddText(zip, "xl/workbook.xml", Workbook);
                AddText(zip, "xl/_rels/workbook.xml.rels", WorkbookRels);
                AddText(zip, "xl/styles.xml", Styles);
                using var sheet = zip.CreateEntry("xl/worksheets/sheet1.xml", CompressionLevel.Optimal).Open();
                await WriteQuerySheetAsync(sheet, columns, rows, ct);
            }
            File.Move(temp, fullPath, true);
        }
        catch
        {
            if (File.Exists(temp)) File.Delete(temp);
            throw;
        }
    }

    private static string Write(
        string monitorName, DateTime checkedAtUtc, IReadOnlyList<string> columns,
        IReadOnlyList<object?[]> rows, string folder, CancellationToken ct)
    {
        Directory.CreateDirectory(folder);
        var stamp = checkedAtUtc.ToLocalTime().ToString("yyyyMMdd_HHmmss", Inv); // invariant: a Persian calendar culture must not change the file name
        var path = UniquePath(folder, $"{SafeFileName(monitorName)}_{stamp}.xlsx");
        var temp = path + ".tmp";
        try
        {
            using (var file = File.Create(temp))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Create))
            {
                AddText(zip, "[Content_Types].xml", ContentTypes);
                AddText(zip, "_rels/.rels", RootRels);
                AddText(zip, "xl/workbook.xml", Workbook);
                AddText(zip, "xl/_rels/workbook.xml.rels", WorkbookRels);
                AddText(zip, "xl/styles.xml", Styles);
                using var sheet = zip.CreateEntry("xl/worksheets/sheet1.xml", CompressionLevel.Optimal).Open();
                WriteSheet(sheet, columns, rows, ct);
            }
            File.Move(temp, path);
        }
        catch
        {
            if (File.Exists(temp)) File.Delete(temp);
            throw;
        }
        return path;
    }

    private static void AddText(ZipArchive zip, string name, string content)
    {
        using var stream = zip.CreateEntry(name, CompressionLevel.Optimal).Open();
        var bytes = Utf8.GetBytes(content);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static void WriteSheet(Stream stream, IReadOnlyList<string> columns, IReadOnlyList<object?[]> rows, CancellationToken ct)
    {
        using var x = XmlWriter.Create(stream, new XmlWriterSettings { Encoding = Utf8, CloseOutput = false });
        x.WriteStartDocument(true);
        x.WriteStartElement("worksheet", Ns);

        x.WriteStartElement("sheetViews", Ns);
        x.WriteStartElement("sheetView", Ns);
        x.WriteAttributeString("rightToLeft", "1");
        x.WriteAttributeString("workbookViewId", "0");
        x.WriteStartElement("pane", Ns);
        x.WriteAttributeString("ySplit", "1");
        x.WriteAttributeString("topLeftCell", "A2");
        x.WriteAttributeString("activePane", "bottomLeft");
        x.WriteAttributeString("state", "frozen");
        x.WriteEndElement(); // pane
        x.WriteEndElement(); // sheetView
        x.WriteEndElement(); // sheetViews

        x.WriteStartElement("cols", Ns);
        x.WriteStartElement("col", Ns);
        x.WriteAttributeString("min", "1");
        x.WriteAttributeString("max", Math.Max(1, columns.Count).ToString(Inv));
        x.WriteAttributeString("width", "22");
        x.WriteAttributeString("customWidth", "1");
        x.WriteEndElement(); // col
        x.WriteEndElement(); // cols

        x.WriteStartElement("sheetData", Ns);
        WriteRow(x, 1, columns.Cast<object?>().ToArray(), header: true);
        for (var i = 0; i < rows.Count; i++)
        {
            if (i % 500 == 0) ct.ThrowIfCancellationRequested();
            WriteRow(x, i + 2, rows[i], header: false);
        }
        x.WriteEndElement(); // sheetData

        x.WriteEndElement(); // worksheet
        x.WriteEndDocument();
    }

    private static async Task WriteQuerySheetAsync(
        Stream stream, IReadOnlyList<string> columns, IAsyncEnumerable<object?[]> rows, CancellationToken ct)
    {
        using var x = XmlWriter.Create(stream, new XmlWriterSettings { Encoding = Utf8, CloseOutput = false });
        x.WriteStartDocument(true);
        x.WriteStartElement("worksheet", Ns);
        x.WriteStartElement("sheetViews", Ns);
        x.WriteStartElement("sheetView", Ns);
        x.WriteAttributeString("rightToLeft", "1");
        x.WriteAttributeString("workbookViewId", "0");
        x.WriteStartElement("pane", Ns);
        x.WriteAttributeString("ySplit", "1");
        x.WriteAttributeString("topLeftCell", "A2");
        x.WriteAttributeString("activePane", "bottomLeft");
        x.WriteAttributeString("state", "frozen");
        x.WriteEndElement();
        x.WriteEndElement();
        x.WriteEndElement();
        x.WriteStartElement("cols", Ns);
        x.WriteStartElement("col", Ns);
        x.WriteAttributeString("min", "1");
        x.WriteAttributeString("max", Math.Max(1, columns.Count).ToString(Inv));
        x.WriteAttributeString("width", "22");
        x.WriteAttributeString("customWidth", "1");
        x.WriteEndElement();
        x.WriteEndElement();
        x.WriteStartElement("sheetData", Ns);
        WriteRow(x, 1, columns.Cast<object?>().ToArray(), header: true);
        var rowNumber = 2;
        await foreach (var row in rows.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (rowNumber >= MaxExcelRows + 1)
                throw new InvalidOperationException("Excel supports at most 1,048,575 data rows per worksheet.");
            WriteRow(x, rowNumber++, row, header: false);
        }
        x.WriteEndElement();
        x.WriteEndElement();
        x.WriteEndDocument();
    }

    private static void WriteRow(XmlWriter x, int rowNumber, object?[] values, bool header)
    {
        x.WriteStartElement("row", Ns);
        x.WriteAttributeString("r", rowNumber.ToString(Inv));
        for (var i = 0; i < values.Length; i++)
            WriteCell(x, ColumnName(i + 1) + rowNumber.ToString(Inv), values[i], header);
        x.WriteEndElement();
    }

    private static void WriteCell(XmlWriter x, string reference, object? value, bool header)
    {
        if (value is null or DBNull) return;

        switch (value)
        {
            case bool b:
                WriteSimple(x, reference, "b", b ? "1" : "0");
                return;
            case long l when l is > MaxExactInteger or < -MaxExactInteger:
                WriteText(x, reference, l.ToString(Inv), header);
                return;
            case byte or short or int or long or decimal:
                WriteSimple(x, reference, null, Convert.ToString(value, Inv) ?? "");
                return;
            case double d when double.IsFinite(d):
                WriteSimple(x, reference, null, d.ToString("R", Inv));
                return;
            case float f when float.IsFinite(f):
                WriteSimple(x, reference, null, f.ToString("R", Inv));
                return;
            default:
                WriteText(x, reference, Describe(value), header);
                return;
        }
    }

    private static void WriteSimple(XmlWriter x, string reference, string? type, string value)
    {
        x.WriteStartElement("c", Ns);
        x.WriteAttributeString("r", reference);
        if (type is not null) x.WriteAttributeString("t", type);
        x.WriteElementString("v", Ns, value);
        x.WriteEndElement();
    }

    private static void WriteText(XmlWriter x, string reference, string text, bool header)
    {
        x.WriteStartElement("c", Ns);
        x.WriteAttributeString("r", reference);
        if (header) x.WriteAttributeString("s", "1");
        x.WriteAttributeString("t", "inlineStr");
        x.WriteStartElement("is", Ns);
        x.WriteStartElement("t", Ns);
        x.WriteAttributeString("xml", "space", null, "preserve");
        x.WriteString(Clean(text));
        x.WriteEndElement(); // t
        x.WriteEndElement(); // is
        x.WriteEndElement(); // c
    }

    private static string Describe(object value) => value switch
    {
        string s => s,
        DateTime d => d.TimeOfDay == TimeSpan.Zero ? d.ToString("yyyy-MM-dd", Inv) : d.ToString("yyyy-MM-dd HH:mm:ss", Inv),
        DateTimeOffset o => o.ToString("yyyy-MM-dd HH:mm:ss zzz", Inv),
        TimeSpan t => t.ToString("c", Inv),
        byte[] bytes => "0x" + Convert.ToHexString(bytes.AsSpan(0, Math.Min(bytes.Length, 16))) + (bytes.Length > 16 ? "…" : ""),
        IFormattable f => f.ToString(null, Inv),
        _ => value.ToString() ?? ""
    };

    /// <summary>Removes characters XML cannot hold and cuts text Excel cannot hold.</summary>
    private static string Clean(string s)
    {
        if (s.Length > MaxCellChars) s = s[..MaxCellChars];
        var sb = new StringBuilder(s.Length);
        for (var i = 0; i < s.Length; i++)
        {
            var c = s[i];
            if (c is '\t' or '\n' or '\r' || (c >= 0x20 && c <= 0xD7FF) || (c >= 0xE000 && c <= 0xFFFD))
                sb.Append(c);
            else if (char.IsHighSurrogate(c) && i + 1 < s.Length && char.IsLowSurrogate(s[i + 1]))
            {
                sb.Append(c).Append(s[i + 1]);
                i++;
            }
        }
        return sb.ToString();
    }

    internal static string ColumnName(int index)
    {
        var name = new StringBuilder();
        while (index > 0)
        {
            index--;
            name.Insert(0, (char)('A' + index % 26));
            index /= 26;
        }
        return name.ToString();
    }

    private static string SafeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Trim().Trim('.');
        if (cleaned.Length == 0) cleaned = "report";
        return cleaned.Length > 60 ? cleaned[..60] : cleaned;
    }

    private static string UniquePath(string folder, string fileName)
    {
        var path = Path.Combine(folder, fileName);
        if (!File.Exists(path)) return path;
        var stem = Path.GetFileNameWithoutExtension(fileName);
        for (var n = 2; ; n++)
        {
            path = Path.Combine(folder, $"{stem}_{n}.xlsx");
            if (!File.Exists(path)) return path;
        }
    }
}
