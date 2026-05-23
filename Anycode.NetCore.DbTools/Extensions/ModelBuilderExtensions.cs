namespace Anycode.NetCore.DbTools.Extensions;

public static class ModelBuilderExtensions
{
	public static void HasPostgresCitextExtension(this ModelBuilder builder)
	{
		builder.HasPostgresExtension("citext");
	}

	public static void HasPostgresTrigramExtension(this ModelBuilder builder)
	{
		builder.HasPostgresExtension("pg_trgm");
	}

	public static PropertyBuilder HasPostgresCitextColumn(this PropertyBuilder propertyBuilder)
	{
		return propertyBuilder.HasColumnType("citext");
	}

	public static IndexBuilder HasPostgresGinIndex(this IndexBuilder indexBuilder)
	{
		return indexBuilder.HasMethod("gin");
	}

	public static IndexBuilder HasPostgresGinTrigramIndex(this IndexBuilder indexBuilder)
	{
		return indexBuilder.HasPostgresGinIndex().HasOperators("gin_trgm_ops");
	}
}