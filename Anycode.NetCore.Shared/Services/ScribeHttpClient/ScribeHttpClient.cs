using CsvHelper.Configuration;
using RestSharp;
using RestSharp.Serializers;
using RestSharp.Serializers.CsvHelper;
using RestSharp.Serializers.Json;

namespace Anycode.NetCore.Shared.Services.ScribeHttpClient;

/// <summary>
/// RestSharp client that caches responses in memory (to not repeat them), file (for debugging), or anywhere else
/// </summary>
public class ScribeHttpClient
{
	private readonly RestClient _client;
	private readonly ScribeRetryPolicy _retryPolicy;
	private readonly List<IScribeHttpSaver> _savers;
	private readonly ILogger<ScribeHttpClient> _log;
	private readonly IScribeHttpRatelimit? _rateLimit;
	private readonly LogLevel _minLogLevel;

	/// <summary>
	/// RestSharp client that caches responses in memory (to not repeat them), file (for debugging), or anywhere else
	/// </summary>
	public ScribeHttpClient(string? baseUrl,
		ILogger<ScribeHttpClient> log,
		IHttpClientFactory? httpClientFactory = null,
		string? httpClientName = null,
		ScribeRetryPolicy? retryPolicy = null,
		IScribeHttpRatelimit? rateLimit = null,
		IEnumerable<IScribeHttpSaver>? savers = null,
		TimeSpan? timeout = null,
		LogLevel minLogLevel = LogLevel.Trace,
		ScribeSerializerType serializerType = ScribeSerializerType.Json,
		string csvDelimiter = ",")
	{
		_log = log;
		_rateLimit = rateLimit;
		_retryPolicy = retryPolicy ?? new ScribeRetryPolicy(0);
		_minLogLevel = minLogLevel;
		_savers = savers?.ToList() ?? [];

		ConfigureSerialization? configureSerialization;
		switch (serializerType)
		{
			case ScribeSerializerType.Json:
				configureSerialization = s => s.UseSystemTextJson(new JsonSerializerOptions
				{
					// public required string Val { get; init; } parsed from JSON can still be null without this, this leads to confusion
					RespectNullableAnnotations = true,
					PropertyNameCaseInsensitive = true,
					NumberHandling = JsonNumberHandling.AllowReadingFromString,
				});
				break;
			case ScribeSerializerType.Csv:
				configureSerialization = s => s.UseCsvHelper(new CsvConfiguration(CultureInfo.InvariantCulture)
				{
					Delimiter = csvDelimiter,
					PrepareHeaderForMatch = args => args.Header.ToLowerInvariant()
				});
				break;
			case ScribeSerializerType.Xml:
				configureSerialization = s => s.UseXml();
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(serializerType), serializerType, "Unknown serializer type");
		}

		var options = new RestClientOptions { Timeout = timeout };
		if (!string.IsNullOrEmpty(baseUrl))
			options.BaseUrl = new Uri(baseUrl);

		_client = httpClientFactory != null
			? new RestClient(httpClientFactory.CreateClient(httpClientName ?? string.Empty), options, configureSerialization: configureSerialization)
			: new RestClient(options, configureSerialization: configureSerialization);
	}

	public Task<ScribeHttpResponse<T>> ExecuteSafeAsync<T>(RestRequest request, CancellationToken ct = default)
		where T : class
	{
		return ExecuteSafeAsync<T>(request, null, ct);
	}

	public async Task<ScribeHttpResponse<T>> ExecuteSafeAsync<T>(RestRequest request,
		Func<T, bool>? checkIfDataValid = null, CancellationToken ct = default)
		where T : class
	{
		var hash = GetRequestHash(request);

		_log.Log(LogLevel.Trace, _minLogLevel,
			"Executing http request to {BaseUrl}/{RequestResource} with parameters: {Parameters}. Request hash: {RequestHash}",
			_client.Options.BaseUrl, request.Resource, string.Join(", ", request.Parameters.Select(x => $"{x.Name}={x.Value}")), hash);
		foreach (var saver in _savers.Where(x => x.CanReturnResponse))
		{
			var response = await saver.GetResponseAsync<T>(hash, ct);
			if (response != null)
			{
				_log.Log(LogLevel.Trace, _minLogLevel, "Returning cached response for request {RequestHash}", hash);
				return new ScribeHttpResponse<T>(response);
			}
		}

		T? result = null;
		string? content = null;

		var triesCount = _retryPolicy.RetriesCount + 1;
		for (var i = 1; i <= triesCount; i++)
		{
			try
			{
				var response = await ExecuteAsync<T>(request, i > 1, ct);
				content = response.Content;
				if (!response.IsSuccessful || response.Data == null)
				{
					if (i == triesCount)
					{
						_log.Log(LogLevel.Trace, _minLogLevel, response.ErrorException,
							"Returning failed http request with hash {RequestHash}. Response: {Response}", hash, content);
						return new ScribeHttpResponse<T>(response);
					}

					_log.Log(LogLevel.Warning, _minLogLevel, response.ErrorException,
						"Failed to execute http request with hash {RequestHash}. Response: {Response}", hash, content);
					continue;
				}

				result = response.Data!;

				if (checkIfDataValid != null && !checkIfDataValid(result)) // Data was unexpected, don't save it
				{
					if (!_retryPolicy.RetryOnInvalidData || i == triesCount)
						throw new HttpRequestException($"Failed to validate data from http request to {_client.Options.BaseUrl}/{request.Resource}");

					_log.Log(LogLevel.Warning, _minLogLevel, "Failed to validate data from http request with hash {RequestHash}. Response: {Response}",
						hash, content);
					continue;
				}

				break;
			}
			catch (Exception e)
			{
				if (i == triesCount)
				{
					_log.Log(LogLevel.Warning, _minLogLevel, e, "Returning failed http request with hash {RequestHash} after {TriesCount} tries.",
						hash, triesCount);
					return new ScribeHttpResponse<T>(e);
				}

				_log.Log(LogLevel.Warning, _minLogLevel, e, "Exception executing http request with hash {RequestHash}", hash);
			}
		}

		foreach (var saver in _savers)
		{
			await saver.SaveResponseAsync(hash, result!, ct);
		}

		// Lots of heavy responses can be returned, so we truncate it for logs
		_log.Log(LogLevel.Trace, _minLogLevel, 3000, "Returning successful http request with hash {RequestHash}. Response: {Response}", hash, content);
		return new ScribeHttpResponse<T>(result!);
	}

	/// <summary>
	/// If data is not valid, it will throw an exception.
	/// </summary>
	public Task<T> ExecuteUnsafeAsync<T>(RestRequest request, CancellationToken ct = default) where T : class
	{
		return ExecuteUnsafeAsync<T>(request, null, ct);
	}

	/// <summary>
	/// If data is not valid, it will throw an exception.
	/// </summary>
	public async Task<T> ExecuteUnsafeAsync<T>(RestRequest request, Func<T, bool>? checkIfDataValid = null,
		CancellationToken ct = default)
		where T : class
	{
		var result = await ExecuteSafeAsync(request, checkIfDataValid, ct);
		if (result.IsSuccessful)
			return result.Data!;

		throw new HttpRequestException($"Failed executing request to {_client.Options.BaseUrl}{request.Resource}. " +
		                               $"Status code: {result.StatusCode}. Content: {result.Content}.", result.ErrorException);
	}

	public async Task CleanCacheAsync(bool forceCleanAll, CancellationToken ct)
	{
		foreach (var saver in _savers)
		{
			await saver.CleanCacheAsync(forceCleanAll, ct);
		}
	}

	/// <summary>
	/// Execute request without expecting response data. Supports retries and rate limiting.
	/// </summary>
	public async Task<ScribeHttpResponse> ExecuteSafeAsync(RestRequest request, CancellationToken ct = default)
	{
		var hash = GetRequestHash(request);

		_log.Log(LogLevel.Trace, _minLogLevel,
			"Executing http request to {BaseUrl}/{RequestResource} with parameters: {Parameters}. Request hash: {RequestHash}",
			_client.Options.BaseUrl, request.Resource, string.Join(", ", request.Parameters.Select(x => $"{x.Name}={x.Value}")), hash);

		return await ExecuteWithRetriesAsync(hash, async isRetry =>
		{
			var response = await ExecuteAsync(request, isRetry, ct);
			return new ScribeHttpResponse(response);
		});
	}

	/// <summary>
	/// Execute request without expecting response data. Throws on failure.
	/// </summary>
	public async Task<ScribeHttpResponse> ExecuteUnsafeAsync(RestRequest request, CancellationToken ct = default)
	{
		var result = await ExecuteSafeAsync(request, ct);
		if (!result.IsSuccessful)
		{
			throw new HttpRequestException($"Failed executing request to {_client.Options.BaseUrl}{request.Resource}. " +
			                               $"Status code: {result.StatusCode}. Content: {result.Content}.", result.ErrorException);
		}

		return result;
	}

	private async Task<TResponse> ExecuteWithRetriesAsync<TResponse>(string hash, Func<bool, Task<TResponse>> executeFunc)
		where TResponse : ScribeHttpResponse
	{
		var triesCount = _retryPolicy.RetriesCount + 1;
		for (var i = 1; i <= triesCount; i++)
		{
			try
			{
				var response = await executeFunc(i > 1);
				if (!response.IsSuccessful)
				{
					if (i == triesCount)
					{
						_log.Log(LogLevel.Trace, _minLogLevel, response.ErrorException,
							"Returning failed http request with hash {RequestHash}. Response: {Response}", hash, response.Content);
						return response;
					}

					_log.Log(LogLevel.Warning, _minLogLevel, response.ErrorException,
						"Failed to execute http request with hash {RequestHash}. Response: {Response}", hash, response.Content);
					continue;
				}

				return response;
			}
			catch (Exception e)
			{
				if (i == triesCount)
				{
					_log.Log(LogLevel.Warning, _minLogLevel, e, "Returning failed http request with hash {RequestHash} after {TriesCount} tries.",
						hash, triesCount);
					return (TResponse)new ScribeHttpResponse(e);
				}

				_log.Log(LogLevel.Warning, _minLogLevel, e, "Exception executing http request with hash {RequestHash}", hash);
			}
		}

		// Should never reach here, but compiler requires it
		throw new InvalidOperationException("Retry loop completed without returning a response");
	}

	private async Task<RestResponse<T>> ExecuteAsync<T>(RestRequest request, bool isRetry, CancellationToken ct)
		where T : notnull
	{
		if (_rateLimit != null)
			await _rateLimit.WaitRatelimitAsync(isRetry, ct);

		return await _client.ExecuteAsync<T>(request, ct);
	}

	private async Task<RestResponse> ExecuteAsync(RestRequest request, bool isRetry, CancellationToken ct)
	{
		if (_rateLimit != null)
			await _rateLimit.WaitRatelimitAsync(isRetry, ct);

		return await _client.ExecuteAsync(request, ct);
	}

	private static string GetRequestHash(RestRequest request)
	{
		var sb = new StringBuilder();
		sb.Append(request.Method.ToString());
		sb.Append(request.Resource);
		foreach (var param in request.Parameters)
		{
			sb.Append('&');
			sb.Append(param.Name);
			if (param.Value != null)
			{
				if (param.Value is string s)
					sb.Append(s);
				else
					sb.Append(JsonSerializer.Serialize(param.Value));
			}
			else
				sb.Append("null");
		}

		return sb.ToString().GetMd5Hash();
	}
}