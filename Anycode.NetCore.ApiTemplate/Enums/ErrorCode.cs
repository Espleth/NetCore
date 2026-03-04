namespace Anycode.NetCore.ApiTemplate.Enums;

public enum ErrorCode
{
	Unknown = 0,

	// Same as HTTP codes
	BadRequest = 400,
	Unauthorized = 401,
	Forbidden = 403,
	NotFound = 404,
	RateLimit = 429,

	// Paging errors
	InvalidPaging = 1000,
	TooLargeTake = 1001,

	// Search/filters errors
	InvalidSearchPattern = 1100,
	InvalidFilters = 1101,

	// User account/registration errors
	InvalidEmail = 2000,
	InvalidUsername = 2001,
	InvalidPassword = 2002,
	InvalidOldPassword = 2003, // When changing password
	EmailIsTaken = 2010,
	UsernameIsTaken = 2011,
	InvalidUsernameOrPassword = 2020,
	UsernamesAreEqual = 2030,
	PasswordsAreEqual = 2031,
	InvalidConfirmationLink = 2040,
	EmailAlreadyConfirmed = 2041,
	EmailNotSet = 2042,
}