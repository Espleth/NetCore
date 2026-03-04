namespace Anycode.NetCore.Shared.Services;

public class LowerInvariantLookupNormalizer : ILookupNormalizer
{
	public string? NormalizeName(string? name)
	{
		return name?.Normalize()?.ToLowerInvariant();
	}

	public string? NormalizeEmail(string? email) => NormalizeName(email);
}