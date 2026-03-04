using CsvHelper;
using CsvHelper.Configuration;

namespace Anycode.NetCore.DatabaseTemplate.DataSeeding;

public class AppDbTranslationsSeeder
{
	private static readonly DateTimeOffset _createdAt = DateTimeOffset.UtcNow;

	public static void SeedData(AppDbContext db)
	{
		var textEntities = new List<TextEntity>();
		var translationEntities = new Dictionary<string, List<TranslationEntity>>();

		using var reader = new StreamReader("Data/translations.csv");
		using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
		{
			HasHeaderRecord = true,
			Delimiter = ",",
		});

		var records = csv.GetRecords<TextRecord>().ToList();

		var recordsDuplicates = records
			.GroupBy(x => (x.Id, x.LanguageId))
			.Where(g => g.Count() > 1)
			.SelectMany(g => g.Skip(1))
			.ToList();

		if (recordsDuplicates.Any())
			throw new InvalidOperationException($"Duplicate records found in translations.csv: {string.Join(", ", recordsDuplicates.Select(x => x.Id))}");

		foreach (var record in records)
		{
			if (textEntities.All(te => te.Id != record.Id))
			{
				textEntities.Add(new TextEntity
				{
					Id = record.Id,
					OriginalLanguageId = 1,
					TranslationsMask = 1,
					DoNotAutoTranslate = true,
					Cacheable = true,
				});
				translationEntities[record.Id] = [];
			}

			translationEntities[record.Id].Add(new TranslationEntity
			{
				TextId = record.Id,
				LanguageId = record.LanguageId,
				Translation = record.Translation.Replace("\r\n", "\n").Trim(),
				CreatedAt = _createdAt,
				Model = "Manual",
			});
		}

		db.Texts.AddRangeIfNotExists<TextEntity, string>(textEntities);
		foreach (var translation in translationEntities.SelectMany(x => x.Value))
		{
			var hasTranslation = db.Translations.Any(x => x.TextId == translation.TextId && x.LanguageId == translation.LanguageId);
			if (hasTranslation)
				db.Translations.Update(translation);
			else
				db.Translations.Add(translation);
		}
	}

	private class TextRecord
	{
		public required string Id { get; init; }
		public int LanguageId { get; init; }
		public required string Translation { get; init; }
	}
}