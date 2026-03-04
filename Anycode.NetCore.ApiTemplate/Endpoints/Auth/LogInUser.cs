namespace Anycode.NetCore.ApiTemplate.Endpoints.Auth;

public class LogInUser : IEndpoint
{
	public void Register(IEndpointRouteBuilder endpointsBuilder)
	{
		endpointsBuilder.MapPost("/v1/auth/login", HandleAsync)
			.ForceNoAuth()
			.WithSummary("Log in")
			.WithDescription("Authorise user by Email or Username. Sets auth cookie")
			.WithTags("Auth");
	}

	private static async Task HandleAsync(AuthRequest body,
		AppDbContext db, AuthHelperService authHelperService, WebContext webContext, LastActivitiesService lastActivitiesService,
		SignInManager<UserEntity> signInManager, UserManager<UserEntity> userManager,
		JwtConfig jwtConfig, CancellationToken ct)
	{
		var user = await authHelperService.GetUserByNameOrEmailAsync(body.Name, true, ct);

		if (user == null)
			throw new AppException(ErrorCode.InvalidUsernameOrPassword);

		// TODO[low] lock out user if too many failed attempts
		var signInResult = await signInManager.CheckPasswordSignInAsync(user, body.Password, false);
		if (!signInResult.Succeeded)
			throw new AppException(ErrorCode.InvalidUsernameOrPassword);

		await userManager.ResetAccessFailedCountAsync(user);
		await lastActivitiesService.UpdateUserLastActivityAsync(user.Id);

		var jwt = AuthHelper.GenerateToken(user, jwtConfig, body.RememberMe);
		webContext.SetAuthorised(jwt);
	}
}