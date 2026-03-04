namespace Anycode.NetCore.ApiTemplate.Services.Email;

public class SendEmailRequestConsumer(ILogger<SendEmailRequestConsumer> log)
	: BaseConsumer<SendEmailRequestConsumer, SendEmailRequest>(log)
{
	protected override Task ConsumeAsync(ConsumerContext<SendEmailRequest> context, CancellationToken ct)
	{
		Log.Info("Sending email {EmailType} to {UserId}", context.Message.Type, context.Message.UserId);
		return Task.CompletedTask;
	}
}