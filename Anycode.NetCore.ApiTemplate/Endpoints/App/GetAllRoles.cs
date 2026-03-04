namespace Anycode.NetCore.ApiTemplate.Endpoints.App;

public class GetAllRoles : IEndpoint
{
	public void Register(IEndpointRouteBuilder endpointsBuilder)
	{
		endpointsBuilder.MapGet("/v1/app/roles", HandleAsync)
			.AllowAnonymous()
			.WithSummary("Get all roles")
			.WithDescription("Get list with all roles in the app")
			.WithTags("App");
	}

	private static async Task<RolesResponse> HandleAsync(AppDbContext db, CancellationToken ct)
	{
		var result = await db.Roles.Select(x => new RoleModel
		{
			RoleId = x.Id,
			Name = x.Name,
			Comment = x.Comment,
			Permissions = x.Permissions.Select(p => p.Permission).ToList(),
		}).ToListAsync(ct);

		return new RolesResponse
		{
			Roles = result,
		};
	}
}