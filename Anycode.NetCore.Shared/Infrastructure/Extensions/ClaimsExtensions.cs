namespace Anycode.NetCore.Shared.Infrastructure.Extensions;

public static class ClaimsExtensions
{
	private const string ClaimTypeId = "sub";

	public static bool TryGetClaimId<T>(this ClaimsPrincipal principal, out T? id) where T : struct, IEquatable<T>
	{
		return TryGetClaimValue(principal, ClaimTypeId, out id);
	}

	private static bool TryGetClaimValue<T>(this ClaimsPrincipal principal, string claimType, out T? value) where T : struct, IEquatable<T>
	{
		var claim = principal.Claims.FirstOrDefault(x => x.Type == claimType);

		if (claim != null && SystemHelpers.TryParse<T>(claim.Value, out var claimValue) && !claimValue.Equals(default(T)))
		{
			value = claimValue;
			return true;
		}

		value = null;
		return false;
	}

	public static bool TryGetClaimValue(this ClaimsPrincipal principal, string claimType, out string? value)
	{
		var claim = principal.Claims.FirstOrDefault(x => x.Type == claimType);

		if (claim != null && !string.IsNullOrEmpty(claim.Value))
		{
			value = claim.Value;
			return true;
		}

		value = null;
		return false;
	}
}