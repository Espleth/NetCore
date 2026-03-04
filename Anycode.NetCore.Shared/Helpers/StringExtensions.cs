namespace Anycode.NetCore.Shared.Helpers;

public static class StringExtensions
{
	extension(string? str)
	{
		public bool ContainsIIC(string value)
		{
			return str?.Contains(value, StringComparison.InvariantCultureIgnoreCase) ?? false;
		}

		public bool EqualsIIC(string value)
		{
			return string.Equals(str, value, StringComparison.InvariantCultureIgnoreCase);
		}
	}

	extension(string str)
	{
		public string ReplaceIIC(string oldValue, string? newValue)
		{
			return str.Replace(oldValue, newValue, StringComparison.InvariantCultureIgnoreCase);
		}

		public bool StartsWithIIC(string value)
		{
			return str.StartsWith(value, StringComparison.InvariantCultureIgnoreCase);
		}

		public bool EndsWithIIC(string value)
		{
			return str.EndsWith(value, StringComparison.InvariantCultureIgnoreCase);
		}

		public string Remove(string value)
		{
			return str.Replace(value, "");
		}

		public string RemoveIIC(string value)
		{
			return str.ReplaceIIC(value, "");
		}

		public int IndexOfIIC(string value)
		{
			return str.IndexOf(value, StringComparison.InvariantCultureIgnoreCase);
		}

		public string Truncate(int maxLength)
		{
			return str.Length <= maxLength ? str : str[..maxLength];
		}
	}

	public static StringBuilder AppendLineStart(this StringBuilder str, string value)
	{
		return str.AppendLine().Append(value);
	}

	public static bool ContainsIIC(this IEnumerable<string> collection, string value)
	{
		return collection.Any(s => s.EqualsIIC(value));
	}

	public static string Join<T>(this IEnumerable<T> collection, string separator)
	{
		return string.Join(separator, collection);
	}
}