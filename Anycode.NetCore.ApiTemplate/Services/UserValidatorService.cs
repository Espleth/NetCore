namespace Anycode.NetCore.ApiTemplate.Services;

public class UserValidatorService(ILookupNormalizer normalizer, AppDbContext db)
{
	public async Task<bool> CheckEmailAsync(string email, CancellationToken ct = default)
	{
		if (!ValidateEmail(email))
			throw new AppException(ErrorCode.InvalidEmail);

		var normalized = normalizer.NormalizeEmail(email);
		return !await db.Users.AnyAsync(x => x.NormalizedEmail == normalized, ct);
	}

	public async Task<bool> CheckUsernameAsync(string username, CancellationToken ct = default)
	{
		if (!ValidateUsername(username))
			throw new AppException(ErrorCode.InvalidUsername);

		var normalized = normalizer.NormalizeName(username);
		return !await db.Users.AnyAsync(x => x.NormalizedUserName == normalized, ct);
	}

	public bool ValidateEmail(string email)
	{
		return DefaultValidation.EmailRegex.IsMatch(email);
	}

	public bool ValidateUsername(string? username)
	{
		if (string.IsNullOrWhiteSpace(username))
			return true;
		return DefaultValidation.UsernameRegex.IsMatch(username);
	}

	public bool ValidatePassword(string password)
	{
		if (password.Length < DefaultValidation.MinPasswordLength)
			return false;

		if (password.All(char.IsDigit))
			return false;

		if (password.Distinct().Count() < DefaultValidation.MinPasswordUniqueChars)
			return false;

		return true;
	}
}