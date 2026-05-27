using CsvHelper;
using CsvHelper.Configuration;
using ClosedXML.Excel;

namespace Anycode.NetCore.Shared.Helpers;

public static class SpreadsheetsHelper
{
	private const int MaxWorksheetNameLength = 31;
	private const string DefaultWorksheetName = "Sheet1";
	private static readonly string[] _csvDelimiterCandidates = [",", ";", "\t", "|"];
	private static readonly char[] _invalidWorksheetNameChars = [':', '\\', '/', '?', '*', '[', ']'];

	public static MemoryStream WriteCsvToStream(List<string> headers, List<List<object?>> rows)
	{
		var stream = new MemoryStream();
		using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
		using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

		foreach (var header in headers)
			csv.WriteField(header);
		csv.NextRecord();

		foreach (var row in rows)
		{
			foreach (var value in row)
			{
				var field = value switch
				{
					null => "",
					CurrencyAmount ca => $"{ca.Amount} {ca.CurrencySymbol}",
					_ => value.ToString() ?? "",
				};
				csv.WriteField(field);
			}

			csv.NextRecord();
		}

		writer.Flush();
		stream.Position = 0;
		return stream;
	}

	public static MemoryStream WriteXlsxToStream(string sheetName, List<string> headers, List<List<object?>> rows)
	{
		using var workbook = new XLWorkbook();
		var worksheet = workbook.Worksheets.Add(GetValidWorksheetName(sheetName));

		for (var i = 0; i < headers.Count; i++)
			worksheet.Cell(1, i + 1).Value = headers[i];

		for (var r = 0; r < rows.Count; r++)
		{
			for (var c = 0; c < rows[r].Count; c++)
			{
				var cell = worksheet.Cell(r + 2, c + 1);
				var value = rows[r][c];

				if (value is CurrencyAmount ca)
				{
					cell.Value = (double)ca.Amount;
					cell.Style.NumberFormat.Format = $"#,##0.00 \"{ca.CurrencySymbol}\"";
				}
				else
				{
					cell.Value = value switch
					{
						null => Blank.Value,
						DateTimeOffset dto => dto.DateTime,
						DateTime dt => dt,
						DateOnly d => d.ToDateTime(),
						IConvertible convertible when Type.GetTypeCode(convertible.GetType()) is >= TypeCode.SByte and <= TypeCode.Decimal
							=> convertible.ToDouble(CultureInfo.InvariantCulture),
						_ => value.ToString(),
					};
				}
			}
		}

		worksheet.Columns().AdjustToContents();

		var stream = new MemoryStream();
		workbook.SaveAs(stream);
		stream.Position = 0;
		return stream;
	}

	private static string GetValidWorksheetName(string? sheetName)
	{
		if (string.IsNullOrWhiteSpace(sheetName))
			return DefaultWorksheetName;

		var name = sheetName.Trim();
		var builder = new StringBuilder(name.Length);
		foreach (var character in name)
		{
			builder.Append(_invalidWorksheetNameChars.Contains(character) || char.IsControl(character)
				? ' '
				: character);
		}

		name = builder.ToString().Trim(' ', '\'');
		if (name.Length > MaxWorksheetNameLength)
			name = name[..MaxWorksheetNameLength].Trim(' ', '\'');

		return string.IsNullOrWhiteSpace(name) ? DefaultWorksheetName : name;
	}

	public static async Task<List<Dictionary<string, string?>>> ReadCsvFileAsync(IFormFile file, bool ignoreCase = true, string? delimiter = null)
	{
		await using var stream = file.OpenReadStream();
		using var reader = new StreamReader(stream);
		var content = await reader.ReadToEndAsync();

		var config = new CsvConfiguration(CultureInfo.InvariantCulture)
		{
			HeaderValidated = null,
			MissingFieldFound = null,
			BadDataFound = null,
			Delimiter = string.IsNullOrWhiteSpace(delimiter) ? DetectCsvDelimiter(content) : delimiter,
		};

		using var csvReader = new StringReader(content);
		using var csv = new CsvReader(csvReader, config);

		var rows = new List<Dictionary<string, string?>>();

		await csv.ReadAsync();
		csv.ReadHeader();

		var headers = csv.HeaderRecord;
		if (headers == null || headers.Length == 0)
			return rows;

		while (await csv.ReadAsync())
		{
			var row = new Dictionary<string, string?>(
				ignoreCase ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture);

			foreach (var header in headers)
			{
				row[header] = csv.GetField<string?>(header);
			}

			rows.Add(row);
		}

		return rows;
	}

	private static string DetectCsvDelimiter(string content)
	{
		// Detect delimiter by scoring common candidates on the first non-empty line.
		// Delimiters inside quoted CSV fields are ignored, so values like "London, Baker Street" do not bias detection.
		var line = content
			.Split(["\r\n", "\n", "\r"], StringSplitOptions.RemoveEmptyEntries)
			.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
		if (line == null)
			return ",";

		var best = _csvDelimiterCandidates
			.Select(delimiter => new
			{
				Delimiter = delimiter,
				Score = CountDelimiterOutsideQuotes(line, delimiter),
			})
			.OrderByDescending(x => x.Score)
			.First();

		return best.Score > 0 ? best.Delimiter : ",";
	}

	private static int CountDelimiterOutsideQuotes(string line, string delimiter)
	{
		var count = 0;
		var inQuotes = false;
		for (var i = 0; i < line.Length; i++)
		{
			if (line[i] == '"')
			{
				if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
				{
					i++;
					continue;
				}

				inQuotes = !inQuotes;
				continue;
			}

			if (!inQuotes && line.AsSpan(i).StartsWith(delimiter, StringComparison.Ordinal))
			{
				count++;
				i += delimiter.Length - 1;
			}
		}

		return count;
	}

	public static async Task<List<Dictionary<string, string?>>> ReadXlsxFileAsync(IFormFile file, bool ignoreCase = true)
	{
		await using var stream = file.OpenReadStream();
		using var workbook = new XLWorkbook(stream);
		var worksheet = workbook.Worksheet(1);

		var rows = new List<Dictionary<string, string?>>();

		var firstRow = worksheet.FirstRowUsed();
		if (firstRow == null)
			return rows;

		var headers = firstRow.CellsUsed().Select(cell => cell.GetString()).ToList();

		if (headers.Count == 0)
			return rows;

		var dataRows = worksheet.RowsUsed().Skip(1);
		foreach (var dataRow in dataRows)
		{
			var row = new Dictionary<string, string?>(
				ignoreCase ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture);

			var cellIndex = 0;
			foreach (var cell in dataRow.CellsUsed())
			{
				if (cellIndex < headers.Count)
				{
					row[headers[cellIndex]] = cell.GetString();
				}

				cellIndex++;
			}

			// Add missing columns as null
			for (var i = cellIndex; i < headers.Count; i++)
			{
				row[headers[i]] = null;
			}

			rows.Add(row);
		}

		return rows;
	}
}