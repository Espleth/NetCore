namespace Anycode.NetCore.Shared.NLogTelegram;

[DataContract]
public class MessageRequest
{
	[DataMember(Name = "chat_id")]
	public required string ChatId { get; set; }

	[DataMember(Name = "text")]
	public required string Text { get; set; }

	[DataMember(Name = "disable_web_page_preview")]
	public string? DisableWebPagePreview { get; set; }

	[DataMember(Name = "reply_to_message_id")]
	public int? ReplyToMessageId { get; set; }
}