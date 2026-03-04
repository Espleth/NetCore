namespace Anycode.NetCore.Shared.NLogTelegram;

public class TelegramClient
{
	public event Action<Exception>? Error;

	private static readonly HttpClient _httpClient = new();

	public void Send(string url)
	{
		try
		{
			_httpClient.GetAsync(url).Wait();
		}
		catch (Exception e)
		{
			OnError(e);
		}
	}

	private void OnError(Exception obj)
	{
		Error?.Invoke(obj);
	}
}