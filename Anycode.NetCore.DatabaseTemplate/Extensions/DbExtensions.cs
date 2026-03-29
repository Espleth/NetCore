namespace Anycode.NetCore.DatabaseTemplate.Extensions;

public static class DbExtensions
{
	/// <summary>
	/// Don't use for migrations outside of this project because it'll use wrong assembly
	/// </summary>
	public static void AddAppDbContext(this IServiceCollection services, string connectionString)
	{
		services.AddAppDbContext<AppDbContext>(connectionString, MapEnums);
	}

	public static NpgsqlDbContextOptionsBuilder MapEnums(this NpgsqlDbContextOptionsBuilder options)
	{
		return options
			.MapEnum<Permission>();
	}
}