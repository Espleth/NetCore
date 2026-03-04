namespace Anycode.NetCore.HealthChecker;

public class HealthCheckerHealthCheck(NpgsqlDataSource dataSource, HealthCheckerConfig config) : IHealthCheck
{
	public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext healthCheckContext, CancellationToken ct = default)
	{
		var lastUpdate = await GetLastUpdateDateAsync(ct);
		return HealthCheckHelper.TimeHealthCheck(config.Name, lastUpdate, TimeSpan.FromMinutes(15), TimeSpan.FromHours(1));
	}

	private async Task<DateTimeOffset?> GetLastUpdateDateAsync(CancellationToken ct)
	{
		await using var conn = await dataSource.OpenConnectionAsync(ct);
		const string sql = $"""
		                    SELECT "{nameof(ConfigurationEntity.Value)}" FROM "{HealthCheckerConstants.ConfigTableName}" 
		                    WHERE "{nameof(ConfigurationEntity.Key)}" = @key LIMIT 1
		                    """;
		await using var cmd = new NpgsqlCommand(sql, conn);
		cmd.Parameters.AddWithValue("@key", HealthCheckerConstants.HealthCheckerLastCheckDate(config.Name));
		if (await cmd.ExecuteScalarAsync(ct) is not string value)
			return null;
		return DateTimeOffset.TryParse(value, out var dto) ? dto : null;
	}
}