namespace Anycode.NetCore.Shared.Models;

public record UserIpInfo
{
	public required string? Ip { get; init; }
	public required string? CountryCode { get; init; }
}