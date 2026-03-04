namespace Anycode.NetCore.ApiTemplate.Endpoints.User;

[PublicAPI]
public record UserRoleResponse
{
	public required int RoleId { get; init; }
	public required string RoleName { get; init; }
	public required List<Permission> Permissions { get; init; }
}