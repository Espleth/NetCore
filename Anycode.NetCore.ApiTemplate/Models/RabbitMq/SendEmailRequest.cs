namespace Anycode.NetCore.ApiTemplate.Models.RabbitMq;

public class SendEmailRequest
{
	public required Guid UserId { get; set; }
	public UserEntity? User { get; set; }
	public required EmailType Type { get; set; }
	public Dictionary<string, string> Params { get; set; } = new();
}

public class SendEmailRequest<T> : SendEmailRequest where T : class
{
	public required T Data { get; set; }
}