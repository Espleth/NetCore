namespace Anycode.NetCore.Shared.Services.ScribeHttpClient;

public class ScribeHttpFileSaver(bool canReturnResponse, string? filePath = null) : IScribeHttpSaver
{
	public const string RequestsGitignoreFolder = "HttpRequests";

	private static readonly JsonSerializerOptions _jsonOptions = new()
	{
		WriteIndented = true,
	};

	public bool CanReturnResponse => canReturnResponse;

	private readonly string _filePath = filePath ?? RequestsGitignoreFolder;

	/// <summary>
	/// TODO[low] ideally save the original json (at least formatted), because we might loose some data
	/// </summary>
	public Task SaveResponseAsync<T>(string hash, T response, CancellationToken ct) where T : class
	{
		var json = JsonSerializer.Serialize(response, _jsonOptions);
		var path = GetPath(hash);
		return Helpers.SystemHelpers.WriteAllTextWithFolderAsync(path, json, ct);
	}

	public async Task<T?> GetResponseAsync<T>(string hash, CancellationToken ct) where T : class
	{
		var path = GetPath(hash);
		if (!File.Exists(path))
			return null;

		var json = await File.ReadAllTextAsync(path, ct);
		var response = JsonSerializer.Deserialize<T>(json);
		return response;
	}

	public Task CleanCacheAsync(bool forceCleanAll, CancellationToken ct)
	{
		if (!forceCleanAll)
			return Task.CompletedTask; // Cleaning files are not required

		throw new NotImplementedException(); // TODO[low] remove saved requests
	}

	private string GetPath(string hash)
	{
		return Path.Combine(_filePath, $"http_{hash}.json");
	}
}