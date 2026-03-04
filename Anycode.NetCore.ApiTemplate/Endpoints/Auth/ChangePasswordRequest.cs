namespace Anycode.NetCore.ApiTemplate.Endpoints.Auth;

[PublicAPI]
public record ChangePasswordRequest
{
	/// <summary>
	/// Not required if user doesn't have password.
	/// </summary>
	public string? CurrentPassword { get; init; }

	public required string NewPassword { get; init; }
}