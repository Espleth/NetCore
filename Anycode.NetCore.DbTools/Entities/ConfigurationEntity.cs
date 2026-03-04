namespace Anycode.NetCore.DbTools.Entities;

public class ConfigurationEntity
{
	[Key]
	public required string Key { get; init; }

	public string? Value { get; set; }
}