namespace Anycode.NetCore.ApiTemplate.Endpoints.Auth;

[PublicAPI]
public record AuthRequest
{
	/// <summary>
	/// Username or email
	/// </summary>
	public required string Name { get; init; }

	public required string Password { get; init; }

	/// <summary>
	/// Remember user
	/// </summary>
	public bool RememberMe { get; init; }
}