namespace Anycode.NetCore.Shared.Models;

public record struct JwtTokenInfo(string Token, DateTimeOffset ExpiresAt);