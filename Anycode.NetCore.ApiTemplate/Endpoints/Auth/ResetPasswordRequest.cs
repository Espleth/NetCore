namespace Anycode.NetCore.ApiTemplate.Endpoints.Auth;

[PublicAPI]
public class ResetPasswordRequest
{
	[MinLength(3)]
	public required string NameOrEmail { get; init; }
	public required string Password { get; init; }
	public required string Token { get; init; }
}