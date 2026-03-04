namespace Anycode.NetCore.Shared.Infrastructure.Extensions;

public static class PrivacyHelper
{
	public static string HideEmail(this string email)
	{
		var parts = email.Split('@');
		return parts.Length < 2 ? email.HideSensitiveData() : $"{parts[0].HideSensitiveData()}@{parts[1].HideSensitiveData()}";
	}

	/// <summary>
	/// For hiding sensitive data like names
	/// </summary>
	public static string HideSensitiveData(this string name)
	{
		if (name.Length <= 2)
		{
			return new string('*', name.Length);
		}

		if (name.Length <= 5)
		{
			return name[..1] + new string('*', name.Length - 2) + name[^1..];
		}

		return name[..2] + new string('*', name.Length - 4) + name[^2..];
	}
}