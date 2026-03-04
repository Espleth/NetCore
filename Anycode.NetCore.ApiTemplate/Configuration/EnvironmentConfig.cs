namespace Anycode.NetCore.ApiTemplate.Configuration;

public class EnvironmentConfig
{
	public required EnvironmentType Environment { get; init; }

	public required List<string> CorsOrigins { get; init; }
	
	public required string ResetPasswordUrl { get; init; }
}