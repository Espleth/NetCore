using RestSharp;
using RestSharp.Serializers.Json;

namespace Anycode.NetCore.HealthChecker;

public class HealthCheckJob(NpgsqlDataSource dataSource, HealthCheckerConfig config, ILogger<HealthCheckJob> log)
	: BaseJob<HealthCheckJob>(log)
{
	private static DateTimeOffset? _lastProblemsReport;

	public override async Task ExecuteAsync(CancellationToken ct)
	{
		const string healthyEmoji = "\ud83d\ude0e";
		const string degradedEmoji = "\ud83e\udd72";
		const string unhealthyEmoji = "\ud83d\ude31";

		Log.Info("Starting health check for {TotalServicesCount} services", config.HealthChecks.Count);

		var results = new List<(string service, HealthCheckResponse response)>();
		var client = new RestClient(new RestClientOptions
		{
			Timeout = TimeSpan.FromSeconds(60),
			RemoteCertificateValidationCallback = (_, _, _, _) => true,
		}, null, s => s.UseSystemTextJson(JsonHelper.JsonApiOptions));

		foreach (var service in config.HealthChecks)
		{
			var request = new RestRequest(service.Url);
			var response = await client.ExecuteAsync<HealthCheckResponse>(request, ct);
			if (!response.IsSuccessful)
			{
				results.Add((service.Name,
					BuildFailedConnectionResponse($"Failed to send health check to {service.Url}. Status code: {response.StatusCode}",
						$"Error: {response.ErrorMessage}. Content: {response.Content}")));
			}
			else if (response.Data == null)
			{
				results.Add((service.Name, BuildFailedConnectionResponse($"Failed to connect to {service.Url}", "Somehow response is null")));
			}
			else
			{
				results.Add((service.Name, response.Data));
			}
		}

		var unhealthy = results.Where(x => x.response.Status != HealthStatus.Healthy).ToList();
		if (unhealthy.Any())
		{
			if (_lastProblemsReport != null && DateTimeOffset.UtcNow - _lastProblemsReport.Value < TimeSpan.FromHours(12))
				return;

			_lastProblemsReport = DateTimeOffset.UtcNow;

			var messages = unhealthy.Select(x => $"Service: {x.service}\nFailed checks:\n" +
			                                     $"{string.Join("\n", x.response.Checks.Where(c => c.Status != HealthStatus.Healthy)
				                                     .Select(c => $"Check: {c.Name}\nStatus: {c.Status}\nDescription:\n{c.Description}"))}");

			var isUnhealthy = unhealthy.Any(x => x.response.Status == HealthStatus.Unhealthy);

			Log.Fatal("{StatusEmoji} [{HealthCheckerName}] {UnhealthyServicesCount} services has problems:\n\n{UnhealthyServices}",
				isUnhealthy ? unhealthyEmoji : degradedEmoji, config.Name, unhealthy.Count, string.Join("\n\n", messages));
		}
		else
		{
			if (_lastProblemsReport != null)
			{
				Log.Fatal("[{HealthCheckerName}] All services are back online {StatusEmoji}", config.Name, healthyEmoji);
				_lastProblemsReport = null;
			}
		}

		await SaveDateAsync(DateTimeOffset.UtcNow, ct);
		Log.Info("Health check finished");
	}

	private async Task SaveDateAsync(DateTimeOffset date, CancellationToken ct)
	{
		await using var conn = await dataSource.OpenConnectionAsync(ct);
		const string sql = $"""
		                    INSERT INTO "{HealthCheckerConstants.ConfigTableName}" ("{nameof(ConfigurationEntity.Key)}", "{nameof(ConfigurationEntity.Value)}") 
		                      VALUES (@key, @val)
		                    ON CONFLICT ("{nameof(ConfigurationEntity.Key)}") 
		                      DO UPDATE SET "{nameof(ConfigurationEntity.Value)}" = EXCLUDED."{nameof(ConfigurationEntity.Value)}"
		                    """;
		await using var cmd = new NpgsqlCommand(sql, conn);
		cmd.Parameters.AddWithValue("@key", HealthCheckerConstants.HealthCheckerLastCheckDate(config.Name));
		cmd.Parameters.AddWithValue("@val", date.ToString("O"));
		await cmd.ExecuteNonQueryAsync(ct);
	}

	private static HealthCheckResponse BuildFailedConnectionResponse(string description, string response)
	{
		return new HealthCheckResponse
		{
			Status = HealthStatus.Unhealthy,
			Duration = TimeSpan.Zero,
			Checks =
			[
				new HealthCheckEntry
				{
					Name = "Connection to service",
					Status = HealthStatus.Unhealthy,
					Description = description,
					Exception = response,
					Duration = TimeSpan.Zero,
				},
			],
		};
	}
}