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
			CheckDigitType.UpuS10 => ValidateMod11(number),
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

	private static bool ValidateDamm(long number)
	{
		// For Damm, running the algorithm over ALL digits (including the check digit) should yield 0
		var interim = 0;
		foreach (var digit in number.ToString().Select(c => c - '0'))
		{
			interim = _dammTable[interim, digit];
		}

		return interim == 0;
	}

	private static long AppendCheckDigit(long number, CheckDigitType checkDigitType)
	{
		return checkDigitType switch
		{
			CheckDigitType.None => number,
			CheckDigitType.Luhn => number * 10 + CalculateLuhnCheckDigit(number),
			CheckDigitType.UpuS10 => number * 10 + CalculateMod11CheckDigit(number),
			CheckDigitType.Damm => number * 10 + CalculateDammCheckDigit(number),
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

	// Totally anti-symmetric quasigroup of order 10 (Damm table)
	private static readonly int[,] _dammTable =
	{
		{ 0, 3, 1, 7, 5, 9, 8, 6, 4, 2 },
		{ 7, 0, 9, 2, 1, 5, 4, 8, 6, 3 },
		{ 4, 2, 0, 6, 8, 7, 1, 3, 5, 9 },
		{ 1, 7, 5, 0, 9, 8, 3, 4, 2, 6 },
		{ 6, 1, 2, 3, 0, 4, 5, 9, 7, 8 },
		{ 3, 6, 7, 4, 2, 0, 9, 5, 8, 1 },
		{ 5, 8, 6, 9, 7, 2, 0, 1, 3, 4 },
		{ 8, 9, 4, 5, 3, 6, 2, 0, 1, 7 },
		{ 9, 4, 3, 8, 6, 1, 7, 2, 0, 5 },
		{ 2, 5, 8, 1, 4, 3, 6, 7, 9, 0 },
	};

	private static int CalculateDammCheckDigit(long number)
	{
		var interim = 0;
		foreach (var digit in number.ToString().Select(c => c - '0'))
		{
			interim = _dammTable[interim, digit];
		}

		return interim;
	}
}