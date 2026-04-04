namespace Anycode.NetCore.Shared.Helpers;

/// <summary>
/// When we need to Fatal log certain operation, but we don't want to do it more than once in a certain time period
/// </summary>
public static class TimedLoggingExtensions
{
	private static readonly ConcurrentDictionary<string, DateTimeOffset> _lastLoggedTimes = new();

	/// <summary>
	/// Fatal log but only once per given time interval, otherwise log as Error
	/// </summary>
	public static void FatalWithInterval(this ILogger log, string logKey, TimeSpan interval,
		[StructuredMessageTemplate] string? message, params object?[] args)
	{
		var now = DateTimeOffset.UtcNow;
		var lastLoggedTime = _lastLoggedTimes.GetOrAdd(logKey, DateTimeOffset.MinValue);
		if (now - lastLoggedTime >= interval)
		{
			log.Fatal(message, args);
			_lastLoggedTimes[logKey] = now;
		}
		else
		{
			log.Error(message, args);
		}
	}
}