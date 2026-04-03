namespace Anycode.NetCore.Shared.Infrastructure.Extensions;

public static class AuthHelper
{
	public static JwtTokenInfo GenerateToken<T>(ISecurityUser<T> user, JwtConfig jwtConfig, TimeSpan? tokenLifetime = null) where T : notnull
	{
		var tokenBuilder = new JwtTokenBuilder(JwtTokenBuilder.CreateSecurityKey(jwtConfig.SecretKey),
			jwtConfig.ValidIssuer, jwtConfig.ValidAudience);

		tokenBuilder.AddClaim("sub", user.Id.ToString()!);
		tokenBuilder.AddClaim("jti", HMAC.HMACSHA256(user.SecurityStamp!, jwtConfig.StampSecretKey));

		var expiresAt = DateTimeOffset.UtcNow.Add(tokenLifetime ?? TimeSpan.FromMinutes(jwtConfig.ExpiryInMinutes));
		tokenBuilder.AddExpiration(tokenLifetime ?? TimeSpan.FromMinutes(jwtConfig.ExpiryInMinutes));

		return new JwtTokenInfo(tokenBuilder.Build(), expiresAt);
	}
}