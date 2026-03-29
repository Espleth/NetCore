namespace Anycode.NetCore.DatabaseTemplate.DataSeeding;

public class AppDbDataSeeder
{
	public static void SeedData(AppDbContext db, ILogger logger)
	{
		logger.LogInformation("Seeding data to the DB...");
		db.Configuration.SetAsync("DbSeedingDate", DateTimeOffset.UtcNow).Wait();
		AppDbCommonDataSeeder.SeedData(db, logger);
		AppDbTranslationsSeeder.SeedData(db, logger);
		db.SaveChanges();
		logger.LogInformation("Database seed completed successfully");
	}
}