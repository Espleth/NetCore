namespace Anycode.NetCore.Shared.Models;

public class RestClaim
{
	public string Type { get; set; } = null!;
	public string Value { get; set; } = null!;

	public RestClaim()
	{
	}

	public RestClaim(Claim claim)
	{
		Type = claim.Type;
		Value = claim.Value;
	}
}