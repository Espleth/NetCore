namespace Anycode.NetCore.Shared.Configuration;

public record JwtConfig
{
	public required string SecretKey { get; init; }
	public required string StampSecretKey { get; init; }
	public required string ValidIssuer { get; init; }
	public required string ValidAudience { get; init; }
	public required List<string> CookieDomains { get; init; }
	public int ExpiryInMinutes { get; set; } = 60;
}