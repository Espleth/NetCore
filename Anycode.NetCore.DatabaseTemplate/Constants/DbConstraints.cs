namespace Anycode.NetCore.DatabaseTemplate.Constants;

public class DbConstraints
{
	public const int MaxEmailLength = 100;
	public const int MaxUsernameLength = 30;

	public const int MaxIpLength = 50; // IPv6 length max 45 symbols
	public const int MaxCountryCodeLength = 10; // Should be 2, but just in case

	public const int MaxUserAgentLength = 1024; // Max length of User-Agent header, trim the rest
	public const int MaxPageLength = 100; // Max length of "/page" part in URLs, trim the rest
	public const int MaxReferrerLength = 200; // Max length of HTTP referrer URL, trim the rest
	public const int MaxUtmLength = 100; // Max length of UTM parameters, trim the rest
}