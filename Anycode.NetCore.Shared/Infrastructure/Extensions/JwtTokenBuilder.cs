namespace Anycode.NetCore.Shared.Infrastructure.Extensions;

public class JwtTokenBuilder(SecurityKey securityKey, string? issuer = null, string? audience = null)
{
	private Dictionary<string, string>? _claims;
	private Dictionary<string, object>? _claimsCollection;

	private DateTime? _expirationDate;
	private DateTime? _notBeforeDate;

	public void AddClaim(string type, string value)
	{
		_claims ??= new Dictionary<string, string>();
		_claims.Add(type, value);
	}

	public void AddClaimCollection(string type, object value)
	{
		_claimsCollection ??= new Dictionary<string, object>();
		_claimsCollection.Add(type, value);
	}

	public void AddExpiration(TimeSpan period)
	{
		_expirationDate = DateTime.UtcNow.Add(period);
	}

	public void AddNotBefore(DateTime date)
	{
		_notBeforeDate = date;
	}

	public string Build()
	{
		var payload = new JwtPayload(issuer,
			audience,
			_claims?.Select(x => new Claim(x.Key, x.Value)),
			_claimsCollection,
			_notBeforeDate,
			_expirationDate,
			DateTime.UtcNow);

		var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
		var securityToken = new JwtSecurityToken(new JwtHeader(credentials), payload);

		return new JwtSecurityTokenHandler().WriteToken(securityToken);
	}

	public static SymmetricSecurityKey CreateSecurityKey(string secret) => new(Encoding.UTF8.GetBytes(secret));
}