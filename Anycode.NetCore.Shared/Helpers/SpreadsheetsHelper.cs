using CsvHelper;
using CsvHelper.Configuration;
using ClosedXML.Excel;
using Anycode.NetCore.Shared.Models;

namespace Anycode.NetCore.Shared.Helpers;

public static class SpreadsheetsHelper
{
	public static MemoryStream WriteCsvToStream(List<string> headers, List<List<object>> rows)
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
				var field = value is CurrencyAmount ca
					? $"{ca.Amount} {ca.CurrencySymbol}"
					: value.ToString() ?? "";
				csv.WriteField(field);
			}

			csv.NextRecord();
		}

		writer.Flush();
		stream.Position = 0;
		return stream;
	}

	public static MemoryStream WriteXlsxToStream(string sheetName, List<string> headers, List<List<object>> rows)
	{
		using var workbook = new XLWorkbook();
		var worksheet = workbook.Worksheets.Add(sheetName);

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

	public static async Task<List<Dictionary<string, string?>>> ReadCsvFileAsync(IFormFile file, bool ignoreCase = true)
	{
		var config = new CsvConfiguration(CultureInfo.InvariantCulture)
		{
			HeaderValidated = null,
			MissingFieldFound = null,
			BadDataFound = null
		};

		await using var stream = file.OpenReadStream();
		using var reader = new StreamReader(stream);
		using var csv = new CsvReader(reader, config);

		var rows = new List<Dictionary<string, string?>>();

		await csv.ReadAsync();
		csv.ReadHeader();

		var headers = csv.HeaderRecord;
		if (headers == null || headers.Length == 0)
			return rows;

		while (await csv.ReadAsync())
		{
			var row = new Dictionary<string, string?>(ignoreCase ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture);

			foreach (var header in headers)
			{
				row[header] = csv.GetField<string?>(header);
			}

			rows.Add(row);
		}

		return rows;
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
			var row = new Dictionary<string, string?>(ignoreCase ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture);

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