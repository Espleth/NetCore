namespace Anycode.NetCore.Shared.Helpers;

public static class SystemHelpers
{
	/// <summary>
	/// Convert string to Guid using (part of) SHA-256 hash
	/// </summary>
	public static Guid ToGuid(this string str)
	{
		var bytes = Encoding.UTF8.GetBytes(str);
		var hash = SHA256.HashData(bytes);
		var guidBytes = hash[..16];
		// Set the version (bits 12-15) to 5 to indicate GUID from hash (xxxxxxxx-xxxx-5xxx-xxxx-xxxxxxxxxxxx)
		guidBytes[7] &= 0x0F;
		guidBytes[7] |= 0x50;
		return new Guid(guidBytes);
	}

	/// <summary>
	/// Not to be used for secure hashes
	/// </summary>
	public static string GetMd5Hash(this string str)
	{
		var inputBytes = Encoding.ASCII.GetBytes(str);
		var hashBytes = MD5.HashData(inputBytes);
		return Convert.ToHexString(hashBytes);
	}

	public static string BuildUrl(string url, IEnumerable<KeyValuePair<string, object>> query)
	{
		var queryStr = string.Join('&', query.Select(x => $"{x.Key}={HttpUtility.UrlEncode(x.Value.ToString())}"));
		return new UriBuilder(url)
		{
			Query = queryStr,
		}.ToString();
	}

	public static Task WriteAllTextWithFolderAsync(string path, string content, CancellationToken ct = default)
	{
		var file = new FileInfo(path);
		if (file.Directory == null)
			throw new ArgumentException($"Invalid file path {path}");

		file.Directory.Create();
		return File.WriteAllTextAsync(file.FullName, content, ct);
	}

	public static bool InRange(this int? value, int minInclusive, int maxInclusive)
	{
		if (value == null)
			return false;
		return value >= minInclusive && value <= maxInclusive;
	}

	public static bool IsNullOrWhiteSpace(this string? value)
	{
		return string.IsNullOrWhiteSpace(value);
	}
	
	public static bool TryParse<T>(string? str, out T res) where T : struct
	{
		res = default!;
		if (string.IsNullOrEmpty(str))
			return false;

		try
		{
			var converter = TypeDescriptor.GetConverter(typeof(T));
			if (converter.CanConvertFrom(typeof(string)))
			{
				res = (T)converter.ConvertFromString(str)!;
				return true;
			}

			return false;
		}
		catch
		{
			return false;
		}
	}
}