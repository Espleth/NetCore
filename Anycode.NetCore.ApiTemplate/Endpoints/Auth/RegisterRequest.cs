namespace Anycode.NetCore.ApiTemplate.Endpoints.Auth;

[PublicAPI]
public record RegisterRequest
{
	[EmailAddress]
	public required string Email { get; init; }
	public string? Username { get; init; }

	public required string Password { get; init; }

	// User shouldn't be able to assign himself roles, but this is for testing purposes
	public required int RoleId { get; init; }
}