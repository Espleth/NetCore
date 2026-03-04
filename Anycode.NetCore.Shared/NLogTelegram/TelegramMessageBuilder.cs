namespace Anycode.NetCore.Shared.NLogTelegram;

public class TelegramMessageBuilder(string baseUrl, string chatId, string text)
{
	private readonly TelegramClient _client = new();
	private readonly MessageRequest _request = new() { Text = text, ChatId = chatId };

	/// <summary>
	/// Telegram message length limit.
	/// </summary>
	private const int MaxTextLength = 4096;

	public static TelegramMessageBuilder Build(string baseUrl, string chatId, string text)
	{
		return new TelegramMessageBuilder(baseUrl, chatId, text);
	}

	public TelegramMessageBuilder OnError(Action<Exception> error)
	{
		_client.Error += error;

		return this;
	}

	public void Send()
	{
		var dic = new Dictionary<string, string>
		{
			{ "chat_id", _request.ChatId },
			{ "text", _request.Text[..Math.Min(MaxTextLength, _request.Text.Length)] }
		};

		var array = dic
			.Select(x => $"{HttpUtility.UrlEncode(x.Key)}={HttpUtility.UrlEncode(x.Value)}")
			.ToArray();

		var url = $"{baseUrl}?{string.Join("&", array)}";

		_client.Send(url);
	}
}