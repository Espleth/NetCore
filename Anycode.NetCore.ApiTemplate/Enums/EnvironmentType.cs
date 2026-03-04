namespace Anycode.NetCore.ApiTemplate.Enums;

public enum EnvironmentType
{
	Unknown = 0,

	/// <summary>
	/// Production. Send emails
	/// </summary>
	Production = 1,

	/// <summary>
	/// Public development environment. Optionally send emails
	/// </summary>
	Development = 2,

	/// <summary>
	/// When testing locally. Optionally send emails, disable response cache
	/// </summary>
	Local = 3,
}