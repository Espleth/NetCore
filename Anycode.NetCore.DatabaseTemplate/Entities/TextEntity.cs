namespace Anycode.NetCore.DatabaseTemplate.Entities;

[Index(nameof(TranslationsMask))]
public class TextEntity : IEntity<string>
{
	[Key]
	[MaxLength(100)]
	public required string Id { get; init; }

	public required int OriginalLanguageId { get; init; }
	public LanguageEntity? OriginalLanguage { get; init; }

	/// <summary>
	/// Sum of all languages ids that have translations for this text. Used for quick check if all translations are present
	/// </summary>
	public required int TranslationsMask { get; set; }

	/// <summary>
	/// If this is a one of the small poll of manual texts that are used in the app frequently and can be cached
	/// List is available through TextLocalizations and cached with TextsCacheService
	/// </summary>
	public required bool Cacheable { get; init; }

	/// <summary>
	/// Exclude this text from auto-translation
	/// </summary>
	public bool DoNotAutoTranslate { get; set; }

	/// <summary>
	/// If text got replaced by another one and probably should not be used anymore
	/// </summary>
	public bool IsObsolete { get; set; }

	[NotMapped]
	public string OriginalText => Translations.First(x => x.LanguageId == OriginalLanguageId).Translation;

	public string GetTranslationOrOriginal(int languageId)
	{
		var translation = Translations.FirstOrDefault(x => x.LanguageId == languageId);
		return translation?.Translation ?? OriginalText;
	}

	public List<TranslationEntity> Translations { get; init; } = [];
}

internal class TextEntityConfiguration : IEntityTypeConfiguration<TextEntity>
{
	public void Configure(EntityTypeBuilder<TextEntity> builder)
	{
		builder.HasOne(x => x.OriginalLanguage)
			.WithMany(x => x.Texts)
			.HasForeignKey(x => x.OriginalLanguageId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}