namespace Anycode.NetCore.Shared.Services.ScribeHttpClient;

public class ScribeHttpBetweenRequestsRatelimit(TimeSpan delayBetweenRequests, TimeSpan? delayBetweenErrorRequests = null) : IScribeHttpRatelimit
{
	private readonly TimeSpan _delayBetweenErrorRequests = delayBetweenErrorRequests ?? delayBetweenRequests;

	private DateTimeOffset? _lastRequestTime;

	// TODO[low] thread-safe
	public async Task WaitRatelimitAsync(bool isRetry, CancellationToken ct)
	{
		var now = DateTimeOffset.UtcNow;
		if (_lastRequestTime == null)
		{
			_lastRequestTime = now;
			return;
		}

		var delay = isRetry ? _delayBetweenErrorRequests : delayBetweenRequests;
		if (now - _lastRequestTime < delay)
			await Task.Delay(delay - (now - _lastRequestTime).Value, ct);

		_lastRequestTime = DateTimeOffset.UtcNow;
	}
}