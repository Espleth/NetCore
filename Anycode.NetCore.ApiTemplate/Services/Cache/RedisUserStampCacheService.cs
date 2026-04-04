using StackExchange.Redis;

namespace Anycode.NetCore.ApiTemplate.Services.Cache;

/// <summary>
/// Preferred implementation of <see cref="IUserStampCacheService{TId}"/> in case if API can be deployed in multiple instances, and we need a shared cache.
/// Note that we're checking stamp on every user request
/// </summary>
public class RedisUserStampCacheService(AppDbContext db, IDatabase redis, JwtConfig jwtConfig)
	: IUserStampCacheService<Guid>
{
	public async Task<UserAuthStampInfo> GetUserStampByIdAsync(Guid userId, CancellationToken ct = default)
	{
		var info = await redis.HashGetAsync(RedisKeys.UsersAuthInfo, userId.ToString());
		if (info.HasValue)
			return UserAuthStampInfo.FromString(info);

		return await UpdateUserAuthInfoAsync(userId, ct);
	}

	private async Task<UserAuthStampInfo> UpdateUserAuthInfoAsync(Guid userId, CancellationToken ct = default)
	{
		var userAuthInfo = await db.Users.Where(x => x.Id == userId)
			.Select(x => new
			{
				x.IsBlocked,
				x.SecurityStamp,
			})
			.FirstOrUnauthorizedAsync(ct);

		var authStamp = userAuthInfo.IsBlocked
			? "-"
			: userAuthInfo.SecurityStamp == null
				? ""
				: HMAC.HMACSHA256(userAuthInfo.SecurityStamp, jwtConfig.StampSecretKey);

		await redis.HashFieldSetAndSetExpiryAsync(RedisKeys.UsersAuthInfo, userId.ToString(), authStamp, TimeSpan.FromHours(8));
		return UserAuthStampInfo.FromString(authStamp);
	}

	public async Task InvalidateStampAsync(Guid userId)
	{
		await redis.HashDeleteAsync(RedisKeys.UsersAuthInfo, userId.ToString());
	}
}