namespace Anycode.NetCore.Shared.Models;

public record UserAuthStampInfo(bool IsBlocked, bool HasPassword, string SecurityStamp)
{
	public static UserAuthStampInfo FromString(string? userAuthInfoStr)
	{
		if (userAuthInfoStr == "-")
			return new UserAuthStampInfo(true, false, "");

		if (string.IsNullOrEmpty(userAuthInfoStr))
			return new UserAuthStampInfo(false, false, "");

		return new UserAuthStampInfo(false, true, userAuthInfoStr);
	}
}