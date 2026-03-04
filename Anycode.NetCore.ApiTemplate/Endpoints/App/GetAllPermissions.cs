namespace Anycode.NetCore.ApiTemplate.Endpoints.App;

public class GetAllPermissions : IEndpoint
{
	public void Register(IEndpointRouteBuilder endpointsBuilder)
	{
		endpointsBuilder.MapGet("/v1/app/permissions", HandleAsync)
			.AllowAnonymous()
			.WithSummary("Get all permissions")
			.WithDescription("Get list with all permissions in the app")
			.WithTags("App");
	}

	private static async Task<PermissionsResponse> HandleAsync(AppDbContext db, CancellationToken ct)
	{
		var result = await db.RolesPermissions.Select(x => new PermissionModel
		{
			Permission = x.Permission,
			Comment = x.Comment,
		}).ToListAsync(ct);

		return new PermissionsResponse
		{
			Permissions = result,
		};
	}
}