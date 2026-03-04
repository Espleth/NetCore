namespace Anycode.NetCore.Shared.Services;

public partial class BotsDetectionService(IHttpContextAccessor httpContextAccessor, ILogger<BotsDetectionService> log)
{
	private static Regex? _crawlerAgentRegex;
	private static HashSet<string>? _crawlerAgentMatches;

	private static readonly Lock _lock = new();

	private bool? _isCrawler;

	private HttpContext HttpContext => httpContextAccessor.HttpContext ?? throw new Exception("HttpContext is null");

	public bool IsCrawler()
	{
		if (_isCrawler != null)
			return _isCrawler.Value;

		_isCrawler = IsCrawlerInternal();
		return _isCrawler.Value;
	}

	/// <summary>
	/// Detects if the current request is from a crawler or bot by user-agent header.
	/// </summary>
	private bool IsCrawlerInternal()
	{
		// Truncate to avoid performance issues with invalid user-agents and compare lowercase
		var userAgent = HttpContext.GetHeader("User-Agent")?.Truncate(500).ToLowerInvariant();
		if (string.IsNullOrEmpty(userAgent))
			return true;

		if (_crawlerAgentMatches == null)
		{
			lock (_lock)
			{
				if (_crawlerAgentMatches == null)
				{
					LoadPatterns();
				}
			}
		}

		foreach (var pattern in _crawlerAgentMatches!)
		{
			if (userAgent.Contains(pattern))
				return true;
		}

		if (_crawlerAgentRegex?.IsMatch(userAgent) == true)
			return true;

		return false;
	}

	private void LoadPatterns()
	{
		try
		{
			// https://github.com/monperrus/crawler-user-agents/blob/master/crawler-user-agents.json
			// TODO[low] load from GitHub
			var path = Path.Combine(AppContext.BaseDirectory, "Config", "crawler-user-agents.json");
			var json = File.ReadAllText(path);

			var patterns = JsonSerializer.Deserialize<List<CrawlerAgent>>(json)!.Select(x => x.Pattern).ToList();

			_crawlerAgentMatches = [];
			var regexPatterns = new List<string>();
			foreach (var patternOriginal in patterns)
			{
				var pattern = patternOriginal.ToLowerInvariant();
				// Determine whether the pattern can be treated as a simple match or needs regex (simple match is faster so we prefer it)

				var clearPattern = pattern.Replace("\\(", "(").Replace("\\)", ")")
					.Replace("\\.", ".")
					.Replace("\\/", "/");

				clearPattern = RegexCaseSelector().Replace(clearPattern, "$1"); // e.g. [hh]ell[oo] -> hello (cuz we ignore case)

				if (clearPattern.All(x => x is >= 'a' and <= 'z' or >= '0' and <= '9' or '.' or '-' or '_' or '/' or '(' or ')' or ' ' or ':' or '@' or '\''))
				{
					// If the pattern contains only alphanumeric characters, dots, dashes, and underscores, no special regex sauce, we can treat it as a simple match
					_crawlerAgentMatches.Add(clearPattern);
				}
				else
				{
					regexPatterns.Add(pattern);
				}
			}

			if (regexPatterns.Any())
				_crawlerAgentRegex = new Regex(string.Join("|", regexPatterns), RegexOptions.IgnoreCase | RegexOptions.Compiled);
		}
		catch (Exception e)
		{
			log.LogCritical(e, "Failed to load crawler agents patterns");
			_crawlerAgentMatches = [];
		}
	}

	[PublicAPI]
	private class CrawlerAgent
	{
		[JsonPropertyName("pattern")]
		public required string Pattern { get; set; }
	}

	[GeneratedRegex(@"\[(.)\1\]")] // e.g. [hh]ell[oo] -> hello (cuz we ignore case)
	private static partial Regex RegexCaseSelector();
}