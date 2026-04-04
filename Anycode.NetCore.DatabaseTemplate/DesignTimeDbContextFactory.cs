namespace Anycode.NetCore.DatabaseTemplate;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
	private static readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(logging =>
	{
		logging.AddSimpleConsole(options =>
		{
			options.SingleLine = true;
			options.TimestampFormat = "HH:mm:ss ";
		});
		logging.SetMinimumLevel(LogLevel.Information);
	});

	public AppDbContext CreateDbContext(string[] args)
	{
		var configuration = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile($"config{Path.DirectorySeparatorChar}appsettings.json")
			.AddJsonFile($"config{Path.DirectorySeparatorChar}appsettings.local.json", true)
			.Build();

		var builder = new DbContextOptionsBuilder<AppDbContext>();
		var seedLogger = _loggerFactory.CreateLogger<AppDbDataSeeder>();
		// Data seeding applied on dotnet ef database update
		// https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding
		builder.UseNpgsql(configuration["ConnectionString"]!, optionsBuilder =>
				optionsBuilder.CommandTimeout(300).MapEnums())
			.UseSeeding((db, _) => AppDbDataSeeder.SeedData(db, seedLogger));
		return new AppDbContext(builder.Options);
	}
}