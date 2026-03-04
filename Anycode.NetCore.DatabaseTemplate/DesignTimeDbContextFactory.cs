namespace Anycode.NetCore.DatabaseTemplate;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
	public AppDbContext CreateDbContext(string[] args)
	{
		var configuration = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile($"config{Path.DirectorySeparatorChar}appsettings.json")
			.AddJsonFile($"config{Path.DirectorySeparatorChar}appsettings.local.json", true)
			.Build();

		var builder = new DbContextOptionsBuilder<AppDbContext>();
		// Data seeding applied on dotnet ef database update
		// https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding
		builder.UseNpgsql(configuration["ConnectionString"]!, optionsBuilder => optionsBuilder.CommandTimeout(300).MapEnums())
			.UseSeeding((db, _) => AppDbDataSeeder.SeedData(db));
		return new AppDbContext(builder.Options);
	}
}