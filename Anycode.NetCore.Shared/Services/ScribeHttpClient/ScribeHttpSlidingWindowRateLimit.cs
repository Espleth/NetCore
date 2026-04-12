namespace Anycode.NetCore.Shared.Services.ScribeHttpClient;

public class ScribeHttpSlidingWindowRateLimit(int maxRequests, TimeSpan window) : IScribeHttpRateLimit
{
	private readonly AsyncLocker _locker = new();
	private readonly Queue<DateTimeOffset> _timestamps = new();

	public async Task WaitRatelimitAsync(bool isRetry, CancellationToken ct)
	{
		while (true)
		{
			using var _ = await _locker.EnterAsync(ct);

			var now = DateTimeOffset.UtcNow;
			var windowStart = now - window;

			// Evict expired timestamps
			while (_timestamps.Count > 0 && _timestamps.Peek() <= windowStart)
				_timestamps.Dequeue();

			if (_timestamps.Count < maxRequests)
			{
				_timestamps.Enqueue(now);
				return;
			}

			// Need to wait until the oldest request falls out of the window
			var waitUntil = _timestamps.Peek() + window;
			var delay = waitUntil - now;

			if (delay > TimeSpan.Zero)
				await Task.Delay(delay, ct);
		}
	}
}