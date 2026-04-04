namespace Anycode.NetCore.Shared.Infrastructure;

public class ApiHealthCheck : IHealthCheck
{
	public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
		CancellationToken cancellationToken = default)
	{
		var lastResponses = RequestLoggingMiddleware.LastResponses.ToList();

		if (lastResponses.Count < 10)
			return Task.FromResult(HealthCheckResult.Healthy("Not enough requests yet"));

		var badRequests = lastResponses.Count(x => x.statusCode is >= 400 and < 500);
		var badRequestsPercent = badRequests / (decimal)lastResponses.Count * 100;

		var serverErrors = lastResponses.Count(x => x.statusCode >= 500);
		var serverErrorsPercent = serverErrors / (decimal)lastResponses.Count * 100;

		var longRequests = lastResponses.Count(x => x.responseTime.TotalSeconds >= 1);
		var longRequestsPercent = longRequests / (decimal)lastResponses.Count * 100;

		var veryLongRequests = lastResponses.Count(x => x.responseTime.TotalSeconds >= 5);
		var veryLongRequestsPercent = veryLongRequests / (decimal)lastResponses.Count * 100;

		var isDegraded = badRequestsPercent >= 20 || longRequestsPercent >= 20 ||
		                 serverErrorsPercent >= 5 || veryLongRequestsPercent >= 5;
		var isUnhealthy = badRequestsPercent >= 50 || longRequestsPercent >= 50 ||
		                  serverErrorsPercent >= 15 || veryLongRequestsPercent >= 15;

		var averageResponseTime = lastResponses.Average(x => x.responseTime.TotalSeconds);

		return Task.FromResult(new HealthCheckResult(
			isUnhealthy ? HealthStatus.Unhealthy : isDegraded ? HealthStatus.Degraded : HealthStatus.Healthy,
			$"Last {lastResponses.Count} requests:\n" +
			$"{badRequests} ({badRequestsPercent:0.##}%) bad requests (4xx)\n" +
			$"{serverErrors} ({serverErrorsPercent:0.##}%) server errors (5xx)\n" +
			$"{longRequests} ({longRequestsPercent:0.##}%) long requests (>1s)\n" +
			$"{veryLongRequests} ({veryLongRequestsPercent:0.##}%) very long requests (>5s)\n" +
			$"Average response time: {averageResponseTime:0.##}s"));
	}
}