using CsvHelper;
using CsvHelper.Configuration;
using ClosedXML.Excel;

namespace Anycode.NetCore.Shared.Helpers;

public static class SpreadsheetsHelper
{
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