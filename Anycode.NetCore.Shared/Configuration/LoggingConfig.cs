namespace Anycode.NetCore.Shared.Configuration;

public record LoggingConfig
{
	public required bool LogApiRequests { get; init; }

	/// <summary>
	/// Normally only for dev envs
	/// </summary>
	public required bool LogRequestHeaders { get; init; }

	/// <summary>
	/// Normally only for dev envs
	/// </summary>
	public required bool LogRequestBody { get; init; }

	public required bool DetectBots { get; init; }

	/// <summary>
	/// Trace requests that are more than 3 minutes long.
	/// Can be disabled for micro-optimization
	/// </summary>
	public required bool TraceLongRequests { get; init; }
}