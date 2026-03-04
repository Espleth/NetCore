namespace Anycode.NetCore.Shared.Services.ScribeHttpClient;

public interface IScribeHttpRatelimit
{
	Task WaitRatelimitAsync(bool isRetry, CancellationToken ct);
}