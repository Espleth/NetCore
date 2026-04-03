namespace Anycode.NetCore.ApiTemplate.Endpoints.Auth;

public class ResetPassword : IEndpoint
{
	public void Register(IEndpointRouteBuilder endpointsBuilder)
	{
		endpointsBuilder.MapPost("/v1/auth/reset-password", HandleAsync)
			.ForceNoAuth()
			.WithSummary("Reset password")
			.WithDescription("Reset password. Returns JWT token and sets auth cookie for dev.");
	}

	private static async Task<string> HandleAsync([FromBody] ResetPasswordRequest body,
		AppDbContext db, WebContext webContext, AuthHelperService authHelperService,
		UserManager<UserEntity> userManager, UserValidatorService userValidator,
		JwtConfig jwtConfig, ILogger<ResetPassword> log, CancellationToken ct)
	{
		var errors = new List<ErrorCode>();

		if (!userValidator.ValidatePassword(body.Password))
			errors.Add(ErrorCode.InvalidPassword);

		errors.ThrowIfAny();

		var user = await authHelperService.GetUserByNameOrEmailAsync(body.NameOrEmail, true, ct);

		if (user == null)
			throw new AppException(ErrorCode.NotFound);

		var resetResult = await userManager.ResetPasswordAsync(user, body.Token, body.Password);
		if (!resetResult.Succeeded)
		{
			log.Warn("Failed to complete registration for {UserId}: {Errors}",
				user.Id, string.Join(", ", resetResult.Errors.Select(x => $"{x.Code}: {x.Description}")));
			throw new AppException(ErrorCode.InvalidConfirmationLink);
		}

		var jwt = AuthHelper.GenerateToken(user, jwtConfig);
		webContext.SetAuthorised(jwt);
		return jwt.Token;
	}
}