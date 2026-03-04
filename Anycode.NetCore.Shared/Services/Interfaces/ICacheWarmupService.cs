namespace Anycode.NetCore.Shared.Services.Interfaces;

public interface ICacheWarmupService
{
	DateTimeOffset LastUpdate { get; }
	TimeSpan? UpdateInterval { get; }

	Task WarmupAsync(CancellationToken ct);
}