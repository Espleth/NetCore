namespace Anycode.NetCore.ApiTemplate.Endpoints.Auth;

public class RegisterUser : IEndpoint
{
	public void Register(IEndpointRouteBuilder endpointsBuilder)
	{
		endpointsBuilder.MapPost("/v1/auth/register", HandleAsync)
			.ForceNoAuth()
			.WithHeaders(DefaultHeaders.UserAgent)
			.WithSummary("Register")
			.WithDescription("Create new user. Sets auth cookie")
			.WithTags("Auth");
	}

	private static async Task HandleAsync(RegisterRequest body,
		AppDbContext db, EmailQueueService emailQueueService, WebContext webContext,
		UserManager<UserEntity> userManager, UserValidatorService userValidator, ILookupNormalizer normalizer,
		UserStampCacheService userStampCache,
		JwtConfig jwtConfig, ILogger<RegisterUser> log, CancellationToken ct)
	{
		var errors = new List<ErrorCode>();

		if (!userValidator.ValidateEmail(body.Email))
			errors.Add(ErrorCode.InvalidEmail);
		if (!userValidator.ValidateUsername(body.Username))
			errors.Add(ErrorCode.InvalidUsername);
		if (!userValidator.ValidatePassword(body.Password))
			errors.Add(ErrorCode.InvalidPassword);

		errors.ThrowIfAny();

		var normalizedEmail = normalizer.NormalizeEmail(body.Email);
		var normalizedUsername = normalizer.NormalizeName(body.Username);

		if (await db.Users.AnyAsync(x => x.NormalizedEmail == normalizedEmail, ct))
			errors.Add(ErrorCode.EmailIsTaken);
		if (await db.Users.AnyAsync(x => x.NormalizedUserName == normalizedUsername, ct))
			errors.Add(ErrorCode.UsernameIsTaken);

		errors.ThrowIfAny();

		var username = body.Username ?? await GenerateRandomUsernameAsync(db, ct);
		var ipInfo = webContext.GetIpInfo();

		var user = new UserEntity
		{
			Id = Guid.CreateVersion7(),
			Email = body.Email,
			UserName = username,
			RegistrationDate = DateTimeOffset.UtcNow,
			LastActivity = DateTimeOffset.UtcNow,
			RegistrationIp = ipInfo.Ip,
			RegistrationCountryCode = ipInfo.CountryCode,
			RegistrationUserAgent = webContext.GetUserAgent(),
			LanguageId = webContext.GetUserLanguageId(),
			RoleId = body.RoleId,
		};

		log.Info("Registering new user {UserId}", user.Id);

		var createResult = await userManager.CreateAsync(user, body.Password);
		if (!createResult.Succeeded)
			throw new Exception(string.Join(", ", createResult.Errors.Select(x => $"{x.Code}: {x.Description}")));

		await emailQueueService.EnqueueRegisterEmailAsync(user.Id, true, ct: CancellationToken.None);

		var jwt = AuthHelper.GenerateToken(user, jwtConfig);
		webContext.SetAuthorised(jwt);
	}

	// TODO[low] some pretty username
	private static async Task<string> GenerateRandomUsernameAsync(AppDbContext db, CancellationToken ct)
	{
		var random = new Random();
		var username = $"User_{random.Next(1000000, 9999999)}";
		while (await db.Users.AnyAsync(x => x.NormalizedUserName == username, ct))
		{
			username = $"User_{random.Next(10000000, 99999999)}";
		}

		return username;
	}
}