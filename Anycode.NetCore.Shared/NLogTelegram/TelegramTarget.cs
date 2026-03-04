using NLog.Common;
using NLog.Targets;

namespace Anycode.NetCore.Shared.NLogTelegram;

[Target("Telegram")]
public class TelegramTarget : TargetWithLayout
{
	private string BaseUrl { get; set; } = "https://api.telegram.org/bot";

	public string? BotToken { get; set; }

	public string? ChatId { get; set; }


	protected override void InitializeTarget()
	{
		if (string.IsNullOrWhiteSpace(BotToken))
			throw new ArgumentOutOfRangeException(nameof(BotToken), "BotToken cannot be empty.");

		if (string.IsNullOrWhiteSpace(ChatId))
			throw new ArgumentOutOfRangeException(nameof(ChatId), "ChatId cannot be empty.");

		base.InitializeTarget();
	}

	protected override void Write(AsyncLogEventInfo info)
	{
		try
		{
			Send(info);
		}
		catch (Exception e)
		{
			info.Continuation(e);
		}
	}

	private void Send(AsyncLogEventInfo info)
	{
		var message = Layout.Render(info.LogEvent);

		var uriBuilder = new UriBuilder(BaseUrl + BotToken);

		uriBuilder.Path += "/sendMessage";

		var url = uriBuilder.Uri.ToString();

		var builder = TelegramMessageBuilder
			.Build(url, ChatId ?? "", message)
			.OnError(e => info.Continuation(e));

		builder.Send();
	}
}