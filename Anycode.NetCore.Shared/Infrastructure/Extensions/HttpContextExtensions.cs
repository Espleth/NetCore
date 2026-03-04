namespace Anycode.NetCore.Shared.Infrastructure.Extensions;

public static class HttpContextExtensions
{
	public static string? GetHeader(this HttpContext context, string headerName)
	{
		var header = context.Request.Headers.FirstOrDefault(x => x.Key.EqualsIIC(headerName)).Value;
		if (header.Count == 0)
			return null;
		return header.ToString();
	}

	public static string? GetXIp(this HttpContext context)
	{
		var forwardedFor = context.GetHeader(DefaultHeaders.NginxForwardedFor);
		if (string.IsNullOrEmpty(forwardedFor))
			return context.GetHeader(DefaultHeaders.NginxIp);

		var forwardedFirst = forwardedFor.Split(',').First().Trim();
		return !forwardedFirst.Contains('.')
			? context.GetHeader(DefaultHeaders.NginxIp)
			: forwardedFirst;
	}
}