namespace Anycode.NetCore.Shared.Services.ScribeHttpClient;

public interface IScribeHttpSaver
{
	/// <summary>
	/// For some cases we want only save response for debugging but don't return it
	/// </summary>
	bool CanReturnResponse { get; }

	Task SaveResponseAsync<T>(string hash, T response, CancellationToken ct) where T : class;

	Task<T?> GetResponseAsync<T>(string hash, CancellationToken ct) where T : class;

	Task CleanCacheAsync(bool forceCleanAll, CancellationToken ct);
}