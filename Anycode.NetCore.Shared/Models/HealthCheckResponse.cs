namespace Anycode.NetCore.Shared.Models;

[PublicAPI]
public record HealthCheckResponse
{
	public HealthStatus Status { get; init; }
	public TimeSpan Duration { get; init; }
	public List<HealthCheckEntry> Checks { get; init; } = [];
}

[PublicAPI]
public record HealthCheckEntry
{
	public required string Name { get; init; }
	public HealthStatus Status { get; init; }
	public string? Description { get; init; }
	public string? Exception { get; init; }
	public TimeSpan Duration { get; init; }
}