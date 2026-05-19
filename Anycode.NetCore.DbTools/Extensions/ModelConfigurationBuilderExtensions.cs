namespace Anycode.NetCore.DbTools.Extensions;

public static class ModelConfigurationBuilderExtensions
{
	public static void ApplyUtcDateTimeOffsetConverter(this ModelConfigurationBuilder configurationBuilder)
	{
		configurationBuilder.Properties<DateTimeOffset>().HaveConversion<UtcDateTimeOffsetConverter>();
	}
}

public sealed class UtcDateTimeOffsetConverter()
	: ValueConverter<DateTimeOffset, DateTimeOffset>(
		x => x.ToUniversalTime(),
		x => x.ToUniversalTime());