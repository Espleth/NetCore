namespace Anycode.NetCore.DatabaseTemplate.Entities;

[Index(nameof(Code), IsUnique = true)]
[Index(nameof(Name), IsUnique = true)]
public class LanguageEntity : IEntity<int>
{
	[Key]
	public int Id { get; init; }

	/// <summary>
	/// Language code in ISO 639-1 format
	/// </summary>
	public required string Code { get; init; }

	public required string Name { get; init; }

	public List<TextEntity> Texts { get; init; } = [];
	public List<TranslationEntity> Translations { get; init; } = [];

	public List<UserEntity> Users { get; init; } = [];
}