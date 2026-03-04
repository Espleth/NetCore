namespace Anycode.NetCore.Shared.Infrastructure.Extensions;

public class HMAC
{
	public static string HMACSHA256(string input, string secret, ByteStringFormat format = ByteStringFormat.Hex)
	{
		var key = Encoding.UTF8.GetBytes(secret);
		using var hm = new HMACSHA256(key);
		var signed = hm.ComputeHash(Encoding.UTF8.GetBytes(input));
		return FormatOutput(signed, format);
	}

	private static string FormatOutput(byte[] bytes, ByteStringFormat format) => format switch
	{
		ByteStringFormat.Hex => Convert.ToHexString(bytes),
		ByteStringFormat.Base64 => Convert.ToBase64String(bytes),
		_ => throw new NotSupportedException($"Output format {format} not supported")
	};
}