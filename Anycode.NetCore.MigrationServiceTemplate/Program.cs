using Anycode.NetCore.DatabaseTemplate;
using Anycode.NetCore.DatabaseTemplate.DataSeeding;
using Anycode.NetCore.DatabaseTemplate.Extensions;
using Anycode.NetCore.Shared;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
builder.ConfigureHostLogging();

var connectionString = builder.Configuration.GetConnectionString("AppDb") ?? throw new InvalidOperationException("ConnectionStrings:AppDb is not configured");

builder.Services.AddDbContext<AppDbContext>(options => { options.UseNpgsql(connectionString, npgsql => npgsql.MapEnums()); });
builder.Services.AddHostedService<MigrationWorker>();

var host = builder.Build();
host.Run();

sealed class MigrationWorker(IServiceProvider services, IHostApplicationLifetime lifetime, ILogger<MigrationWorker> logger) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await using var scope = services.CreateAsyncScope();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

		logger.LogInformation("Applying migrations...");
		await db.Database.MigrateAsync(stoppingToken);
		logger.LogInformation("Migrations applied successfully");

		logger.LogInformation("Applying data seed...");
		AppDbDataSeeder.SeedData(db, logger);
		logger.LogInformation("Data seed applied successfully");

		lifetime.StopApplication();
	}
}