namespace Anycode.NetCore.ApiTemplate.Endpoints.App;

[PublicAPI]
public record PermissionsResponse
{
	public required List<PermissionModel> Permissions { get; init; }
}

[PublicAPI]
public record PermissionModel
{
	public required Permission Permission { get; init; }
	public required string Comment { get; init; }
}