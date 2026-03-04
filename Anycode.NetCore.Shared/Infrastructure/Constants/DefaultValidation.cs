namespace Anycode.NetCore.Shared.Infrastructure.Constants;

public static class DefaultValidation
{
	// User
	public const int MinPasswordLength = 6;
	public const int MinPasswordUniqueChars = 3;

	public static readonly Regex EmailRegex = new(@"^(?=.{5,100}$)[^@\s]+@[^@\s]+\.[^@\s]+$",
		RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));

	public const string UsernameRegexString = "^(?=.{3,30}$)[a-zA-Z0-9_.-]+$";

	public static readonly Regex UsernameRegex = new(UsernameRegexString,
		RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));

	// Search
	public const int MinSearchLength = 2;
	public const int MaxSearchLength = 30;

	// Paging
	public const int MaxTake = 100;
}