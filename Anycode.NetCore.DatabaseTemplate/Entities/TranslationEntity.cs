namespace Anycode.NetCore.DatabaseTemplate.Entities;

[PrimaryKey(nameof(TextId), nameof(LanguageId))]
public class TranslationEntity
{
	[MaxLength(100)]
	public string TextId { get; init; } = null!;

	public TextEntity? Text { get; init; }

	public required int LanguageId { get; init; }

	[ForeignKey(nameof(LanguageId))]
	public LanguageEntity? Language { get; init; }

	[MaxLength(10000)]
	public required string Translation { get; set; }

	public required DateTimeOffset CreatedAt { get; set; }

	/// <summary>
	/// What is used for translating text
	/// </summary>
	[MaxLength(100)]
	public required string? Model { get; set; }
}

internal class TranslationEntityConfiguration : IEntityTypeConfiguration<TranslationEntity>
{
	public void Configure(EntityTypeBuilder<TranslationEntity> builder)
	{
		builder.HasOne(x => x.Text)
			.WithMany(x => x.Translations)
			.HasForeignKey(x => x.TextId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne(x => x.Language)
			.WithMany(x => x.Translations)
			.HasForeignKey(x => x.LanguageId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}