namespace Anycode.NetCore.Shared.Services.Interfaces;

public interface IUserStampCacheService<TId> where TId : struct
{
	Task<UserAuthStampInfo> GetUserStampByIdAsync(TId userId, CancellationToken ct = default);
	Task InvalidateStampAsync(TId userId);
}