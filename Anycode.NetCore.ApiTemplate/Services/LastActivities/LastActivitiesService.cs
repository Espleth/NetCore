using StackExchange.Redis;

namespace Anycode.NetCore.ApiTemplate.Services.LastActivities;

public class LastActivitiesService(IDatabase redisDb)
{
	public async Task<Dictionary<Guid, DateTimeOffset>> GetUsersLastActivitiesAsync()
	{
		var lastActivities = await redisDb.HashGetAllAsync(RedisKeys.UsersLastActivity);
		return lastActivities.ToDictionary(
			x => Guid.Parse(x.Name.ToString()), x => DateTimeOffset.FromUnixTimeMilliseconds((long)x.Value));
	}

	public async Task UpdateUserLastActivityAsync(Guid userId)
	{
		await redisDb.HashSetAsync(RedisKeys.UsersLastActivity, userId.ToString(),
			DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), When.Always, CommandFlags.FireAndForget);
		await redisDb.HashFieldExpireAsync(RedisKeys.UsersLastActivity, [userId.ToString()],
			TimeSpan.FromDays(7), ExpireWhen.Always, CommandFlags.FireAndForget);
	}
}