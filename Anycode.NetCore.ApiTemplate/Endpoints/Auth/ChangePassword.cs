namespace Anycode.NetCore.ApiTemplate.Endpoints.Auth;

public class ChangePassword : IEndpoint
{
	public void Register(IEndpointRouteBuilder endpointsBuilder)
	{
		endpointsBuilder.MapPost("/v1/auth/change-password", HandleAsync)
			.WithSummary("Change password")
			.WithErrorsDescription("Current password not required if user don't have password. Sets auth cookie.",
				ErrorCode.PasswordsAreEqual, ErrorCode.InvalidPassword, ErrorCode.InvalidOldPassword)
			.WithTags("Auth");
	}

	private static async Task HandleAsync(ChangePasswordRequest body,
		AppDbContext db, UserContext userContext, WebContext webContext, UserStampCacheService userStampCacheService,
		UserManager<UserEntity> userManager, UserValidatorService userValidator,
		JwtConfig jwtConfig, CancellationToken ct)
	{
		var user = await db.Users.AsTracking().FirstOrUnauthorizedAsync(x => x.Id == userContext.UserId, ct);

		if (body.CurrentPassword == body.NewPassword)
			throw new AppException(ErrorCode.PasswordsAreEqual);

		if (!userValidator.ValidatePassword(body.NewPassword))
			throw new AppException(ErrorCode.InvalidPassword);

		if (user.PasswordHash != null)
		{
			if (body.CurrentPassword == null)
				throw new AppException(ErrorCode.InvalidOldPassword);

			var result = await userManager.ChangePasswordAsync(user, body.CurrentPassword, body.NewPassword);
			if (!result.Succeeded)
				throw new AppException(ErrorCode.InvalidOldPassword);
		}
		else
		{
			if (body.CurrentPassword != null)
				throw new AppException(ErrorCode.InvalidOldPassword);

			var result = await userManager.AddPasswordAsync(user, body.NewPassword);
			if (!result.Succeeded)
				throw new AppException(ErrorCode.InvalidPassword);
		}

		await userStampCacheService.InvalidateStampAsync(user.Id);

		var jwt = AuthHelper.GenerateToken(user, jwtConfig, true);
		webContext.SetAuthorised(jwt);
	}
}