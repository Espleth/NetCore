namespace Anycode.NetCore.Shared.Services.ScribeHttpClient;

public interface IScribeHttpRateLimit
{
	Task WaitRatelimitAsync(bool isRetry, CancellationToken ct);
}