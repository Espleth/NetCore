namespace Anycode.NetCore.Shared.Helpers;

[SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
public class SingleWorkingService
{
	private volatile bool _isWorking;

	public async Task RunAsync(Func<Task> func, string workName, ILogger? log, bool throwOnError = false, bool isFatal = false)
	{
		if (_isWorking)
		{
			log?.Warn("Failed to {WorkName}: already working.", workName);
			return;
		}

		lock (this)
		{
			if (_isWorking)
			{
				log?.Warn("Failed to {WorkName}: already working.", workName);
				return;
			}

			_isWorking = true;
		}

		try
		{
			log?.Info("Starting {WorkName}...", workName);
			await func();
		}
		catch (Exception e)
		{
			var level = isFatal ? LogLevel.Critical : LogLevel.Error;
			log?.Log(level, e, "Failed to {WorkName}: {ExceptionMessage}", workName, e.Message);
			if (throwOnError)
				throw;
		}
		finally
		{
			_isWorking = false;
			log?.Info("Finished to {WorkName}.", workName);
		}
	}
}