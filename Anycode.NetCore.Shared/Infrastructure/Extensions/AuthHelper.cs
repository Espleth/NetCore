namespace Anycode.NetCore.Shared.Infrastructure.Extensions;

public static class AuthHelper
{
	public static JwtTokenInfo GenerateToken<T>(ISecurityUser<T> user, JwtConfig jwtConfig, bool permanent = false) where T : notnull
	{
		var tokenBuilder = new JwtTokenBuilder(JwtTokenBuilder.CreateSecurityKey(jwtConfig.SecretKey),
			jwtConfig.ValidIssuer, jwtConfig.ValidAudience);

		tokenBuilder.AddClaim("sub", user.Id.ToString()!);
		tokenBuilder.AddClaim("jti", HMAC.HMACSHA256(user.SecurityStamp!, jwtConfig.StampSecretKey));

		var expiresAt = permanent
			? DateTimeOffset.UtcNow.AddDays(730)
			: DateTimeOffset.UtcNow.AddMinutes(jwtConfig.ExpiryInMinutes);
		tokenBuilder.AddExpiration(permanent
			? TimeSpan.FromDays(730)
			: TimeSpan.FromMinutes(jwtConfig.ExpiryInMinutes));

		return new JwtTokenInfo(tokenBuilder.Build(), expiresAt);
	}
}