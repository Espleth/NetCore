using Quartz;
using IStartupLogger = NLog.ILogger;

var (app, _) = StartupHelper.CreateWebApplication(args, ConfigureServices);

ConfigureApplication();

await app.LaunchAsync();
return;

void ConfigureServices(IServiceCollection services, IConfigurationManager configuration, IStartupLogger startupLog)
{
	var connections = configuration.GetConfig<DbConnections>();

	services.AddSingleton<NpgsqlDataSource>(_ => NpgsqlDataSource.Create(connections.AppDb!));

	var config = services.AddConfig<HealthCheckerConfig>(configuration);
	services.AddHealthChecks().AddCheck<HealthCheckerHealthCheck>($"Health checker '{config.Name}'");

	ConfigureQuartz(services, configuration, startupLog);
}

void ConfigureQuartz(IServiceCollection services, IConfigurationManager configuration, IStartupLogger startupLog)
{
	var config = configuration.GetConfig<HealthCheckerConfig>();
	services.AddQuartz(q => { q.AddJobIfPresent<HealthCheckJob>(startupLog, config.HealthCheckerCron); });

	services.AddQuartzHostedService(q =>
	{
		q.AwaitApplicationStarted = true;
		q.WaitForJobsToComplete = true;
	});
}

void ConfigureApplication()
{
	app.UseHealthChecksExt();
}