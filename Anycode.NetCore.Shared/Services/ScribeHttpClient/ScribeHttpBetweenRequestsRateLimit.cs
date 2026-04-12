namespace Anycode.NetCore.Shared.Services.ScribeHttpClient;

public class ScribeHttpBetweenRequestsRateLimit(
	TimeSpan delayBetweenRequests,
	TimeSpan? delayBetweenErrorRequests = null)
	: IScribeHttpRateLimit
{
	private readonly TimeSpan _delayBetweenErrorRequests = delayBetweenErrorRequests ?? delayBetweenRequests;
	private readonly AsyncLocker _locker = new();

	private DateTimeOffset? _lastRequestTime;

	public async Task WaitRatelimitAsync(bool isRetry, CancellationToken ct)
	{
		using var _ = await _locker.EnterAsync(ct);

		if (_lastRequestTime != null)
		{
			var elapsed = DateTimeOffset.UtcNow - _lastRequestTime.Value;
			var delay = isRetry ? _delayBetweenErrorRequests : delayBetweenRequests;

			if (elapsed < delay)
				await Task.Delay(delay - elapsed, ct);
		}

		_lastRequestTime = DateTimeOffset.UtcNow;
	}
}