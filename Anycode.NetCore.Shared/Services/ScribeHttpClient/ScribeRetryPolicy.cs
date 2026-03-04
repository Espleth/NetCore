namespace Anycode.NetCore.Shared.Services.ScribeHttpClient;

public record ScribeRetryPolicy(int RetriesCount = 3, bool RetryOnInvalidData = true);