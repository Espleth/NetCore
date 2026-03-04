namespace Anycode.NetCore.Shared.Helpers;

public static class IdsGenerator
{
	public static async Task<string> GenerateRandomUsernameAsync<TUser, TKey>(DbSet<TUser> usersSet, CancellationToken ct) where TUser : IdentityUser<TKey>
		where TKey : IEquatable<TKey>
	{
		var random = new Random();
		var username = $"User_{random.Next(1000000, 9999999)}";
		while (await usersSet.AnyAsync(x => x.NormalizedUserName == username, ct))
		{
			username = $"User_{random.Next(10000000, 99999999)}";
		}

		return username;
	}

	public static async Task<int> GenerateIntIdAsync<TEntity>(DbSet<TEntity> set, int digits, int? prefix = null,
		CheckDigitType checkDigitType = CheckDigitType.None, int attempts = 1000, CancellationToken ct = default)
		where TEntity : class, IEntity<int>
	{
		if (digits is < 1 or > 9)
			throw new ArgumentOutOfRangeException(nameof(digits), "Digits must be between 1 and 9");

		int baseId = 0, min, max;
		var hasCheckDigit = checkDigitType != CheckDigitType.None;

		if (prefix > 0)
		{
			var prefixDigits = (int)Math.Floor(Math.Log10(prefix.Value)) + 1;
			var availableDigits = hasCheckDigit ? digits - 1 : digits;
			if (prefixDigits >= availableDigits)
				throw new ArgumentException(
					$"Prefix has {prefixDigits} digits, but available digits is {availableDigits}. Prefix must have fewer digits than available.",
					nameof(prefix));

			var randomDigits = availableDigits - prefixDigits;
			var multiplier = (int)Math.Pow(10, randomDigits);
			baseId = prefix.Value * multiplier;
			min = (int)Math.Pow(10, randomDigits - 1);
			max = multiplier;
		}
		else
		{
			var randomDigits = hasCheckDigit ? digits - 1 : digits;
			min = (int)Math.Pow(10, randomDigits - 1);
			max = (int)Math.Pow(10, randomDigits);
		}

		var rawId = baseId + Random.Shared.Next(min, max);
		var id = (int)AppendCheckDigit(rawId, checkDigitType);
		var attemptCount = 0;
		while (await set.AnyAsync(x => x.Id == id, ct))
		{
			if (++attemptCount >= attempts)
				throw new Exception($"Failed to generate unique ID after {attempts} attempts");

			rawId = baseId + Random.Shared.Next(min, max);
			id = (int)AppendCheckDigit(rawId, checkDigitType);
		}

		return id;
	}

	public static async Task<long> GenerateLongIdAsync<TEntity>(DbSet<TEntity> set, int digits, long? prefix = null,
		CheckDigitType checkDigitType = CheckDigitType.None, int attempts = 1000, CancellationToken ct = default)
		where TEntity : class, IEntity<long>
	{
		if (digits is < 1 or > 18)
			throw new ArgumentOutOfRangeException(nameof(digits), "Digits must be between 1 and 18");

		long baseId = 0, min, max;
		var hasCheckDigit = checkDigitType != CheckDigitType.None;

		if (prefix > 0)
		{
			var prefixDigits = (int)Math.Floor(Math.Log10(prefix.Value)) + 1;
			var availableDigits = hasCheckDigit ? digits - 1 : digits;
			if (prefixDigits >= availableDigits)
				throw new ArgumentException(
					$"Prefix has {prefixDigits} digits, but available digits is {availableDigits}. Prefix must have fewer digits than available.",
					nameof(prefix));

			var randomDigits = availableDigits - prefixDigits;
			var multiplier = (long)Math.Pow(10, randomDigits);
			baseId = prefix.Value * multiplier;
			min = (long)Math.Pow(10, randomDigits - 1);
			max = multiplier;
		}
		else
		{
			var randomDigits = hasCheckDigit ? digits - 1 : digits;
			min = (long)Math.Pow(10, randomDigits - 1);
			max = (long)Math.Pow(10, randomDigits);
		}

		var rawId = baseId + Random.Shared.NextInt64(min, max);
		var id = AppendCheckDigit(rawId, checkDigitType);
		var attemptCount = 0;
		while (await set.AnyAsync(x => x.Id == id, ct))
		{
			if (++attemptCount >= attempts)
				throw new Exception($"Failed to generate unique ID after {attempts} attempts");

			rawId = baseId + Random.Shared.NextInt64(min, max);
			id = AppendCheckDigit(rawId, checkDigitType);
		}

		return id;
	}

	public static bool ValidateCheckDigit(long number, CheckDigitType checkDigitType)
	{
		return checkDigitType switch
		{
			CheckDigitType.None => true,
			CheckDigitType.Luhn => ValidateLuhn(number),
			CheckDigitType.Mod11 => ValidateMod11(number),
			_ => throw new ArgumentOutOfRangeException(nameof(checkDigitType), checkDigitType, null)
		};
	}

	private static bool ValidateLuhn(long number)
	{
		var checkDigit = (int)(number % 10);
		var calculatedCheckDigit = CalculateLuhnCheckDigit(number / 10);
		return checkDigit == calculatedCheckDigit;
	}

	private static bool ValidateMod11(long number)
	{
		var checkDigit = (int)(number % 10);
		var calculatedCheckDigit = CalculateMod11CheckDigit(number / 10);
		return checkDigit == calculatedCheckDigit;
	}

	private static long AppendCheckDigit(long number, CheckDigitType checkDigitType)
	{
		return checkDigitType switch
		{
			CheckDigitType.None => number,
			CheckDigitType.Luhn => number * 10 + CalculateLuhnCheckDigit(number),
			CheckDigitType.Mod11 => number * 10 + CalculateMod11CheckDigit(number),
			_ => throw new ArgumentOutOfRangeException(nameof(checkDigitType), checkDigitType, null)
		};
	}

	private static int CalculateLuhnCheckDigit(long number)
	{
		var sum = 0;
		var isDouble = true;

		while (number > 0)
		{
			var digit = (int)(number % 10);
			number /= 10;

			if (isDouble)
			{
				digit *= 2;
				if (digit > 9)
					digit -= 9;
			}

			sum += digit;
			isDouble = !isDouble;
		}

		return (10 - sum % 10) % 10;
	}

	private static int CalculateMod11CheckDigit(long number)
	{
		int[] weights = [8, 6, 4, 2, 3, 5, 9, 7];

		var digits = number.ToString().Select(c => c - '0').ToArray();
		var sum = 0;

		for (var i = 0; i < digits.Length; i++)
		{
			sum += digits[i] * weights[i % weights.Length];
		}

		var remainder = sum % 11;
		var result = 11 - remainder;

		return result switch
		{
			10 => 0,
			11 => 5,
			_ => result
		};
	}
}