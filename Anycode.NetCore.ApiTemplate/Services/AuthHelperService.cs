namespace Anycode.NetCore.ApiTemplate.Services;

public class AuthHelperService(AppDbContext db, UserValidatorService userValidator, ILookupNormalizer normalizer)
{
	public async Task<UserEntity?> GetUserByNameOrEmailAsync(string nameOrEmail, bool throwOnErrors, CancellationToken ct)
	{
		var isEmail = nameOrEmail.Contains('@');
		if (isEmail && !userValidator.ValidateEmail(nameOrEmail))
		{
			if (throwOnErrors)
				throw new AppException(ErrorCode.InvalidUsernameOrPassword);
			return null;
		}

		if (!isEmail && !userValidator.ValidateUsername(nameOrEmail))
		{
			if (throwOnErrors)
				throw new AppException(ErrorCode.InvalidUsernameOrPassword);
			return null;
		}

		var nameNormalized = normalizer.NormalizeName(nameOrEmail);

		var user = isEmail
			? await db.Users.AsTracking().FirstOrDefaultAsync(x => x.NormalizedEmail == nameNormalized, ct)
			: await db.Users.AsTracking().FirstOrDefaultAsync(x => x.NormalizedUserName == nameNormalized, ct);

		return user;
	}
}