namespace Anycode.NetCore.ApiTemplate.Configuration;

public record EnvironmentConfig
{
	public required EnvironmentType Environment { get; init; }

	public required List<string> CorsOrigins { get; init; }

	public required string ResetPasswordUrl { get; init; }

	public string? OpenApiRoutePrefix { get; init; }
}