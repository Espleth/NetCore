namespace Anycode.NetCore.Shared.Infrastructure.Middlewares;

public class RequestLoggingMiddleware(
	RequestDelegate next,
	IServiceProvider serviceProvider,
	LoggingConfig loggingConfig,
	ILogger<RequestLoggingMiddleware> log)
{
	public static readonly ConcurrentFixedSizedQueue<(int statusCode, TimeSpan responseTime)> LastResponses = new(300);

	public async Task InvokeAsync(HttpContext context)
	{
		// In case of not logging we ideally should save LastResponses. But if that'll be needed, LastResponses should be moved to another middleware
		if (!loggingConfig.LogApiRequests || context.Request.Method == "OPTIONS")
		{
			await next(context);
			return;
		}

		var stopwatch = new Stopwatch();
		stopwatch.Start();

		var scopes = new List<IDisposable?>();

		if (loggingConfig.DetectBots)
		{
			var botsDetector = serviceProvider.GetRequiredService<BotsDetectionService>();
			var isCrawler = botsDetector.IsCrawler();
			scopes.Add(log.BeginScope("{IsCrawler}", isCrawler));
		}

		var url = context.Request.GetDisplayUrl();
		var host = context.Request.Host.Host.ToLowerInvariant();
		var ip = host.EndsWith(".ru")
			? context.GetXIp() // Nginx or other similar service
			: context.GetHeader(DefaultHeaders.CloudflareIp);

		ip ??= context.Connection.RemoteIpAddress?.ToString() ?? "-";

		if (loggingConfig.LogRequestHeaders)
			scopes.Add(log.BeginScope("{Headers}", string.Join(';', context.Request.Headers.Select(x => $"{x.Key}={x.Value}"))));

		if (loggingConfig.LogRequestBody)
		{
			var body = await ReadRequestBodyAsync(context.Request);
			if (!string.IsNullOrEmpty(body))
				body = ReplacePassword(body);
			scopes.Add(log.BeginScope("{Body}", body));
		}

		log.Trace("{Method}: {Url} | IP: {Ip}", context.Request.Method, url, ip);

		// Log scoped info only in "Started" log
		foreach (var scope in scopes)
		{
			scope?.Dispose();
		}

		Timer? timer = null;
		try
		{
			if (loggingConfig.TraceLongRequests)
			{
				timer = new Timer(_ => log.Warn("Request stuck for {ElapsedTotalMinutes:0.#} minutes", stopwatch.Elapsed.TotalMinutes),
					null, TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
			}

			await next(context);
		}
		finally
		{
			if (timer != null)
				await timer.DisposeAsync();

			stopwatch.Stop();
		}

		// Ignore malicious requests from bots, but still log them just in case
		if (!url.StartsWith("https:///") && !url.StartsWith("http:///"))
			LastResponses.Enqueue((context.Response.StatusCode, stopwatch.Elapsed));

		var logErrorFeature = context.Features.Get<LogErrorFeature>();
		if (logErrorFeature != null)
		{
			log.Trace("Finished in {RequestTime:0.###}s | {Method} {Url} | StatusCode: {StatusCode} | Error: {Error}",
				stopwatch.Elapsed.TotalSeconds, context.Request.Method, url, context.Response.StatusCode, logErrorFeature.Error);
		}
		else
		{
			log.Trace("Finished in {RequestTime:0.###}s | {Method} {Url} | StatusCode: {StatusCode}",
				stopwatch.Elapsed.TotalSeconds, context.Request.Method, url, context.Response.StatusCode);
		}
	}

	private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
	{
		request.EnableBuffering();
		request.Body.Position = 0;
		var result = await new StreamReader(request.Body).ReadToEndAsync();
		request.Body.Position = 0;

		return result;
	}

	private static string ReplacePassword(string body)
	{
		var pass = "\"password\":";
		var index = body.IndexOf(pass, StringComparison.OrdinalIgnoreCase);
		if (index > 0)
		{
			var nextIndex = body.IndexOf("\",", index, StringComparison.Ordinal);
			if (nextIndex < 0)
				nextIndex = body.IndexOf("\"}", index, StringComparison.Ordinal); // In case if pass is the last arg

			if (nextIndex > 0)
				body = body.Remove(index + pass.Length + 2, nextIndex - index - pass.Length - 2);
		}

		return body;
	}
}