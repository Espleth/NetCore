using Anycode.NetCore.DatabaseTemplate;
using Anycode.NetCore.DatabaseTemplate.Extensions;
using Anycode.NetCore.Shared;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
builder.ConfigureHostLogging();

var connectionString = builder.Configuration.GetConnectionString("AppDb")
                       ?? throw new InvalidOperationException("ConnectionStrings:AppDb is not configured");

builder.Services.AddDbContext<AppDbContext>(options => { options.UseNpgsql(connectionString, npgsql => npgsql.MapEnums()); });
builder.Services.AddHostedService<MigrationWorker>();

var host = builder.Build();
host.Run();

internal sealed class MigrationWorker(IServiceProvider services, IHostApplicationLifetime lifetime) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await using var scope = services.CreateAsyncScope();
		await services.MigrateAndSeedAppDbAsync(stoppingToken);
		lifetime.StopApplication();
	}
}