using MassTransit;

namespace Anycode.NetCore.ApiTemplate.Services.Email;

public class EmailQueueService(IPublishEndpoint publisher, ILogger<EmailQueueService> log)
{
	public async Task EnqueueRegisterEmailAsync(Guid userId, bool needEmailConfirm, bool throwOnError = false,
		CancellationToken ct = default)
	{
		try
		{
			var request = new SendEmailRequest
			{
				UserId = userId,
				Type = needEmailConfirm ? EmailType.RegistrationConfirm : EmailType.RegistrationWelcome,
			};
			await publisher.Publish(request, ct);
		}
		catch (OperationCanceledException) when (ct.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception e)
		{
			if (throwOnError)
				throw;

			log.Error(e, "Failed to send registration email to user {UserId}: {ExceptionMessage}", userId, e.Message);
		}
	}

	public async Task EnqueueResetPasswordEmailAsync(Guid userId, CancellationToken ct)
	{
		var request = new SendEmailRequest
		{
			UserId = userId,
			Type = EmailType.ResetPassword,
		};
		await publisher.Publish(request, ct);
	}
}