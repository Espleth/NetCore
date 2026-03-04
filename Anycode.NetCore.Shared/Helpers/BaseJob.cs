using Quartz;

namespace Anycode.NetCore.Shared.Helpers;

public abstract class BaseJob<T>(ILogger<T> log) : IJob where T : BaseJob<T>
{
	protected ILogger<T> Log => log;

	public async Task Execute(IJobExecutionContext context)
	{
		using var scope = log.BeginScope("Job {JobType}, id {JobId}", typeof(T).Name, Guid.CreateVersion7());
		try
		{
			await ExecuteAsync(context.CancellationToken);
		}
		catch (Exception e)
		{
			log.Fatal(e, "Failed to execute job: {JobType}", typeof(T).Name);
		}
	}

	public abstract Task ExecuteAsync(CancellationToken ct);
}