namespace Anycode.NetCore.Shared.Services.ScribeHttpClient;

public class ScribeHttpMemCacheSaver(bool canReturnResponse, TimeSpan cacheLifetime) : IScribeHttpSaver
{
	private static readonly IMemoryCache _requestsCache = new MemoryCache(new MemoryCacheOptions());

	public bool CanReturnResponse => canReturnResponse;

	public Task SaveResponseAsync<T>(string hash, T response, CancellationToken ct) where T : class
	{
		_requestsCache.Set(hash, response, cacheLifetime);
		return Task.CompletedTask;
	}

	public Task<T?> GetResponseAsync<T>(string hash, CancellationToken ct) where T : class
	{
		if (_requestsCache.TryGetValue<T>(hash, out var response))
			return Task.FromResult(response);
		return Task.FromResult<T?>(null);
	}

	public Task CleanCacheAsync(bool forceCleanAll, CancellationToken ct)
	{
		((MemoryCache)_requestsCache).Clear();
		return Task.CompletedTask;
	}
}