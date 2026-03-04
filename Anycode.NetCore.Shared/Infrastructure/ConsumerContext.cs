namespace Anycode.NetCore.Shared.Infrastructure;

public record ConsumerContext<T>
{
	public required T Message { get; init; }
}