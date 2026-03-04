namespace Anycode.NetCore.Shared.Infrastructure.Constants;

public class DefaultHeaders
{
	public const string UserAgent = "User-Agent";

	public const string CloudflareIp = "CF-Connecting-IP";
	public const string CloudflareCountryCode = "CF-IPCountry";

	public const string NginxIp = "X-Real-IP";
	public const string NginxForwardedFor = "X-Forwarded-For";
	public const string NginxCountryCode = "X-IPCountry";
}