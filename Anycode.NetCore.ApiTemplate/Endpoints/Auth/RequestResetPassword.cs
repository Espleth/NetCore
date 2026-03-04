namespace Anycode.NetCore.ApiTemplate.Endpoints.Auth;

public class RequestResetPassword : IEndpoint
{
	public void Register(IEndpointRouteBuilder endpointsBuilder)
	{
		endpointsBuilder.MapPost("/v1/auth/request-reset-password", HandleAsync)
			.ForceNoAuth()
			.WithSummary("Request password reset")
			.WithDescription("Request password reset. Sends a password reset email.");
	}

	private static async Task HandleAsync([FromBody] RequestResetPasswordRequest body,
		AppDbContext db, EmailQueueService emailQueueService,
		UserManager<UserEntity> userManager, ILookupNormalizer normalizer,
		EnvironmentConfig envConfig, CancellationToken ct)
	{
		var normalizedEmail = normalizer.NormalizeEmail(body.Email);
		var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, ct);

		// For security reasons, don't reveal if email exists or not
		if (user == null)
			return;

		await emailQueueService.EnqueueResetPasswordEmailAsync(user.Id, ct);
	}
}

public record RequestResetPasswordRequest([EmailAddress] string Email);