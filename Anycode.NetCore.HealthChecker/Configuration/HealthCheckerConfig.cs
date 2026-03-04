namespace Anycode.NetCore.HealthChecker.Configuration;

public class HealthCheckerConfig
{
	public required string Name { get; init; }
	public required string HealthCheckerCron { get; init; }
	public required List<HealthCheckConfigEntry> HealthChecks { get; init; }
}

public class HealthCheckConfigEntry
{
	public required string Name { get; init; }
	public required string Url { get; init; }
}