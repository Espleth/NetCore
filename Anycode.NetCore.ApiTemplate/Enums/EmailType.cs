namespace Anycode.NetCore.ApiTemplate.Enums;

public enum EmailType
{
	Unknown = 0,

	/// <summary>
	/// Welcome to platform, email confirmation not required (registered via social network)
	/// </summary>
	RegistrationWelcome = 1,

	/// <summary>
	/// Welcome to platform, please confirm your email
	/// </summary>
	RegistrationConfirm = 2,

	/// <summary>
	/// Reset user's password
	/// </summary>
	ResetPassword = 3,
}