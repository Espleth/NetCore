namespace Anycode.NetCore.DatabaseTemplate.DataSeeding;

public class AppDbDataSeeder
{
	public static void SeedData(AppDbContext db)
	{
		Console.WriteLine("Seeding data to the DB...");
		db.Configuration.SetAsync("DbSeedingDate", DateTimeOffset.UtcNow).Wait();
		AppDbCommonDataSeeder.SeedData(db);
		AppDbTranslationsSeeder.SeedData(db);
		db.SaveChanges();
	}
}