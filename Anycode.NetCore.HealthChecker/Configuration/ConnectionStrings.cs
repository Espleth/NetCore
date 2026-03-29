namespace Anycode.NetCore.HealthChecker.Configuration;

public record ConnectionStrings
{
	public string? AppDb { get; init; }
}