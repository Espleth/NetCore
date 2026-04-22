namespace Anycode.NetCore.ApiTemplate.Endpoints.User;

public class GetUserRoles : IEndpoint
{
	public void Register(IEndpointRouteBuilder endpointsBuilder)
	{
		endpointsBuilder.MapPost("/v1/user/role", HandleAsync)
			.WithSummary("User role")
			.WithDescription("Get info about current user role and permissions")
			.WithTags("User");
	}

	public static Task<UserRoleResponse> HandleAsync(AppDbContext db, UserContext userContext, CancellationToken ct)
	{
		return db.Users.Where(x => x.Id == userContext.UserIdAuthorized)
			.Select(x => new UserRoleResponse
			{
				RoleId = x.RoleId,
				RoleName = x.Role!.Name,
				Permissions = x.Role!.Permissions.Select(p => p.Permission).ToList(),
			}).FirstAsync(ct);
	}
}