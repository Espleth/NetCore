using Quartz;
using IStartupLogger = NLog.ILogger;

namespace Anycode.NetCore.Shared.Helpers;

public static class QuartzExtensions
{
	public static void AddJobIfPresent<TJob>(this IServiceCollectionQuartzConfigurator quartz,
		IStartupLogger log, string? cron, string? jobName = null)
		where TJob : BaseJob<TJob>
	{
		jobName ??= typeof(TJob).Name; // nameof is not working for generic types

		if (string.IsNullOrEmpty(cron))
		{
			log.Info($"Skipping {jobName} job setup because cron is not set.");
			return;
		}

		log.Info($"Setting up {jobName} job with cron '{cron}'.");

		var jobKey = new JobKey(jobName);
		quartz.AddJob<TJob>(opts => opts.WithIdentity(jobKey));

		quartz.AddTrigger(opts =>
		{
			var options = opts.ForJob(jobKey)
				.WithIdentity($"{jobName}-trigger");

			if (cron == "startup") // Allow "startup" instead of some cron expression to run once at startup for local testing
				options.StartNow();
			else
				options.WithCronSchedule(cron); // Cron with seconds support
		});
	}
}