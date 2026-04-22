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

	/// <summary>
	/// Applies pending EF Core migrations for <see cref="AppDbContext"/> and seeds initial data.
	/// Should be called once on application startup before the host starts serving requests.
	/// </summary>
	public static async Task MigrateAndSeedAppDbAsync(this IServiceProvider services, CancellationToken ct = default)
	{
		await using var scope = services.CreateAsyncScope();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

		logger.LogInformation("Applying migrations...");
		await db.Database.MigrateAsync(ct);
		logger.LogInformation("Migrations applied successfully");

		// If a migration recreated PG types (e.g. an enum via DROP + CREATE TYPE), their OIDs change.
		// Pooled connections still hold the previously cached OID mapping, so subsequent queries fail with
		// "Reading as '...' is not supported for fields having DataTypeName '-.-'".
		// Reload the type mapping on the live connection and clear all pools so every future connection picks up the fresh OIDs.
		var conn = (NpgsqlConnection)db.Database.GetDbConnection();
		var openedHere = conn.State != ConnectionState.Open;
		if (openedHere)
			await conn.OpenAsync(ct);
		await conn.ReloadTypesAsync(ct);
		if (openedHere)
			await conn.CloseAsync();
		NpgsqlConnection.ClearAllPools();

		AppDbDataSeeder.SeedData(db, logger);
	}
}