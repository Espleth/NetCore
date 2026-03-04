namespace Anycode.NetCore.ApiTemplate.Endpoints.App;

[PublicAPI]
public record RolesResponse
{
	public required List<RoleModel> Roles { get; init; }
}

[PublicAPI]
public record RoleModel
{
	public required int RoleId { get; init; }
	public required string Name { get; init; }
	public required string Comment { get; init; }
	public required List<Permission> Permissions { get; init; }
}