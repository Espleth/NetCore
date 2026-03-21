using Quartz;

namespace Anycode.NetCore.Shared.Helpers;

public abstract class BaseJob<T>(ILogger<T> log) : IJob where T : BaseJob<T>
{
	protected virtual bool AllowConcurrentExecution => false;
	protected virtual string JobName => typeof(T).Name;

	protected ILogger<T> Log => log;

	// ReSharper disable once StaticMemberInGenericType
	private static readonly SingleWorkingService _singleWorkingService = new();

	public async Task Execute(IJobExecutionContext context)
	{
		using var scope = log.BeginScope("Job {JobType}, id {JobId}", JobName, Guid.CreateVersion7());
		try
		{
			if (!AllowConcurrentExecution)
			{
				await _singleWorkingService.RunAsync(async () => await ExecuteAsync(context.CancellationToken), JobName, log);
			}
			else
			{
				await ExecuteAsync(context.CancellationToken);
			}
		}
		catch (Exception e)
		{
			log.Fatal(e, "Failed to execute job: {JobType}", JobName);
		}
	}

	public abstract Task ExecuteAsync(CancellationToken ct);
}