using MassTransit;

namespace Anycode.NetCore.Shared.Infrastructure;

public abstract class BaseConsumer<T, TMessage>(ILogger<T> log) : IConsumer<TMessage>
	where TMessage : class
{
	protected ILogger<T> Log => log;

	public async Task Consume(ConsumeContext<TMessage> context)
	{
		try
		{
			await ConsumeAsync(new ConsumerContext<TMessage>
			{
				Message = context.Message,
			}, context.CancellationToken);
		}
		catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception e)
		{
			log.Error(e, "Error while consuming message {MessageName}", nameof(TMessage));
			throw;
		}
	}

	protected abstract Task ConsumeAsync(ConsumerContext<TMessage> context, CancellationToken ct);
}