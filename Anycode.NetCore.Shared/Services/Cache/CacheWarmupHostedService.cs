namespace Anycode.NetCore.Shared.Services.Cache;

public class CacheWarmupHostedService(IServiceProvider serviceProvider, ILogger<CacheWarmupHostedService> log)
	: IHostedService, IAsyncDisposable
{
	private Timer? _updateTimer;
	private static readonly AsyncLocker _locker = new();

	public async Task StartAsync(CancellationToken ct)
	{
		log.Info("Staring cache warmup...");

		await using var scope = serviceProvider.CreateAsyncScope();
		var cacheServices = scope.ServiceProvider.GetServices<ICacheWarmupService>();
		foreach (var service in cacheServices)
		{
			log.Info("Staring cache warmup for {WarmupService}...", service.GetType().Name);
			await service.WarmupAsync(ct);
		}

		_updateTimer = new Timer(_ => OnCheckCacheUpdateAsync(ct), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

		log.Info("Cache warmup done");
	}

	private async void OnCheckCacheUpdateAsync(CancellationToken ct)
	{
		try
		{
			using var _ = await _locker.EnterAsync(ct);

			await using var scope = serviceProvider.CreateAsyncScope();
			var cacheServices = scope.ServiceProvider.GetServices<ICacheWarmupService>();
			foreach (var service in cacheServices)
			{
				if (service.UpdateInterval == null)
					continue;

				if (service.LastUpdate + service.UpdateInterval < DateTimeOffset.UtcNow)
				{
					log.Info("Updating cache for {WarmupService}...", service.GetType().Name);
					await service.WarmupAsync(ct);
				}
			}
		}
		catch (OperationCanceledException) when (ct.IsCancellationRequested)
		{
			// Normal shutdown
		}
		catch (Exception e)
		{
			log.Error(e, "Error during cache update");
		}
	}

	public async Task StopAsync(CancellationToken ct)
	{
		await DisposeAsync();
	}

	public async ValueTask DisposeAsync()
	{
		if (_updateTimer != null)
		{
			await _updateTimer.DisposeAsync();
			_updateTimer = null;
		}
	}
}