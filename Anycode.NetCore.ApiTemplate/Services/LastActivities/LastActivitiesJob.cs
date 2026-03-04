namespace Anycode.NetCore.ApiTemplate.Services.LastActivities;

public class LastActivitiesJob(LastActivitiesService lastActivitiesService, AppDbContext db, ILogger<LastActivitiesJob> log) : BaseJob<LastActivitiesJob>(log)
{
	public override async Task ExecuteAsync(CancellationToken ct)
	{
		Log.Info("Updating last activities for users and sessions...");

		var lastUsersActivities = await lastActivitiesService.GetUsersLastActivitiesAsync();

		foreach (var userActivity in lastUsersActivities)
		{
			await db.Users.Where(x => x.Id == userActivity.Key)
				.ExecuteUpdateAsync(x => x.SetProperty(p => p.LastActivity, userActivity.Value), ct);
		}

		var last24H = DateTimeOffset.UtcNow.AddDays(-1);
		var last7D = DateTimeOffset.UtcNow.AddDays(-7);
		var last30D = DateTimeOffset.UtcNow.AddDays(-30);

		var activeUsersIn24H = await db.Users.CountAsync(x => x.LastActivity > last24H, ct);
		var activeUsersIn7D = await db.Users.CountAsync(x => x.LastActivity > last7D, ct);
		var activeUsersIn30D = await db.Users.CountAsync(x => x.LastActivity > last30D, ct);

		Log.Info("Updating last activities for {UsersCount} users. " +
		         "Active last 24h: {ActiveLast24H}. Last 7 days: {ActiveLast7D}. Last 30 days: {ActiveLast30D}.",
			lastUsersActivities.Count, activeUsersIn24H, activeUsersIn7D, activeUsersIn30D);

		db.ActiveUsersStats.Add(new ActiveUsersStatsEntity
		{
			Date = DateTimeOffset.UtcNow,
			OnlyRegistered = true,
			DailyActiveUsers = activeUsersIn24H,
			WeeklyActiveUsers = activeUsersIn7D,
			MonthlyActiveUsers = activeUsersIn30D,
		});

		await db.SaveChangesAsync(CancellationToken.None);
	}
}