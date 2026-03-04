namespace Anycode.NetCore.ApiTemplate.Services.Cache;

/// <summary>
/// Preferred implementation of <see cref="IUserStampCacheService{TId}"/> in case if API will be running on a single instance,
/// and we can rely on in-memory cache.
/// </summary>
public class UserStampCacheService(AppDbContext db, JwtConfig jwtConfig) : IUserStampCacheService<Guid>
{
	private static readonly MemoryCache _stampByIdCache = new(new MemoryCacheOptions());

	public async Task<UserAuthStampInfo> GetUserStampByIdAsync(Guid userId, CancellationToken ct = default)
	{
		var success = _stampByIdCache.TryGetValue(userId, out UserAuthStampInfo? stampInfo);
		if (success)
			return stampInfo!;

		var authInfo = await db.Users
			.Where(x => x.Id == userId)
			.Select(x => new
			{
				x.IsBlocked,
				x.SecurityStamp
			})
			.FirstOrUnauthorizedAsync(ct);

		if (authInfo.IsBlocked)
			stampInfo = new UserAuthStampInfo(true, false, "");
		else if (authInfo.SecurityStamp == null)
			stampInfo = new UserAuthStampInfo(false, false, "");
		else
			stampInfo = new UserAuthStampInfo(false, true, HMAC.HMACSHA256(authInfo.SecurityStamp!, jwtConfig.StampSecretKey));

		_stampByIdCache.Set(userId, stampInfo, TimeSpan.FromHours(8));
		return stampInfo;
	}

	public Task InvalidateStampAsync(Guid userId)
	{
		_stampByIdCache.Remove(userId);
		return Task.CompletedTask;
	}
}