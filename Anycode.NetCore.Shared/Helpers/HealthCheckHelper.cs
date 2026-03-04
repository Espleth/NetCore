namespace Anycode.NetCore.Shared.Helpers;

public static class HealthCheckHelper
{
	public static HealthCheckResult CombineToSingleHealthCheck(this ICollection<HealthCheckResult> healthCheckResults)
	{
		if (healthCheckResults.All(x => x.Status == HealthStatus.Healthy))
			return HealthCheckResult.Healthy("All checks passed.");

		var description = string.Join('\n', healthCheckResults.Where(x => x.Status != HealthStatus.Healthy).Select(x => x.Description));
		return healthCheckResults.Any(x => x.Status == HealthStatus.Unhealthy)
			? HealthCheckResult.Unhealthy(description)
			: HealthCheckResult.Degraded(description);
	}

	public static HealthCheckResult TimeHealthCheck(string name, DateTimeOffset? lastUpdateDate,
		TimeSpan degradedThreshold, TimeSpan unhealthyThreshold)
	{
		var description = lastUpdateDate != null
			? $"{name} last update: {lastUpdateDate:O}. " +
			  $"No updates in the last {(DateTimeOffset.UtcNow - lastUpdateDate.Value).TotalHours:0.#} hours."
			: $"{name} last update: never.";
		if (lastUpdateDate == null || DateTimeOffset.UtcNow - lastUpdateDate.Value > unhealthyThreshold)
			return HealthCheckResult.Unhealthy(description);

		return DateTimeOffset.UtcNow - lastUpdateDate.Value < degradedThreshold
			? HealthCheckResult.Healthy(description)
			: HealthCheckResult.Degraded(description);
	}
}