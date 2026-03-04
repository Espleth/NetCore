namespace Anycode.NetCore.Shared.Helpers;

public static class AttemptedRequest
{
	/// <summary>
	/// Execute func. If failed, retry after delay.
	/// Throws if failed after attemptsCount attempts
	/// </summary>
	/// <param name="func">func to execute</param>
	/// <param name="log">logger for logging failed attempts. Not required</param>
	/// <param name="attemptsCount">if null, execute indefinitely</param>
	/// <param name="secondsBetweenAttempts">delay between attempts</param>
	/// <param name="doubleDelayEachAttempt">the delay can be doubled after every attempt</param>
	/// <param name="allowedExceptions">if receive exception to this type, throw immediately</param>
	/// <param name="ct">cancellation token</param>
	public static async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> func, ILogger? log,
		int? attemptsCount = 3, double secondsBetweenAttempts = 5, bool doubleDelayEachAttempt = true, ICollection<Type>? allowedExceptions = null,
		CancellationToken ct = default)
	{
		var attempts = 0;
		var currentDelay = secondsBetweenAttempts;
		while (true)
		{
			try
			{
				return await func();
			}
			catch (Exception e)
			{
				if (allowedExceptions != null && allowedExceptions.Contains(e.GetType()))
					throw;

				if (attempts >= attemptsCount)
					throw;

				log?.Warn(e, "Failed to execute attempted request. Attempt {Attempt} of {AttemptsCount}. " +
				             "Retrying in {CurrentDelay} seconds. Error: {ErrorMessage}",
					attempts + 1, attemptsCount, currentDelay, e.Message);

				attempts++;
				await Task.Delay(TimeSpan.FromSeconds(currentDelay), ct);
				if (doubleDelayEachAttempt)
					currentDelay *= 2;
			}
		}
	}

	/// <summary>
	/// Execute func. If failed, retry after delay.
	/// Returns (false, default) if failed after attemptsCount attempts
	/// </summary>
	/// <param name="func">func to execute</param>
	/// <param name="log">logger for logging failed attempts. Not required</param>
	/// <param name="attemptsCount">if null, execute indefinitely</param>
	/// <param name="secondsBetweenAttempts">delay between attempts</param>
	/// <param name="doubleDelayEachAttempt">the delay can be doubled after every attempt</param>
	/// <param name="ct">cancellation token</param>
	public static async Task<(bool, TResult?)> ExecuteSafeAsync<TResult>(Func<Task<TResult>> func, ILogger? log,
		int? attemptsCount = 3, double secondsBetweenAttempts = 5, bool doubleDelayEachAttempt = true, CancellationToken ct = default)
	{
		try
		{
			var result = await ExecuteAsync(func, log, attemptsCount, secondsBetweenAttempts, doubleDelayEachAttempt, null, ct);
			return (true, result);
		}
		catch (Exception e)
		{
			log?.Error(e, "Failed to execute attempted request. Error: {ErrorMessage}", e.Message);
			return (false, default);
		}
	}
}