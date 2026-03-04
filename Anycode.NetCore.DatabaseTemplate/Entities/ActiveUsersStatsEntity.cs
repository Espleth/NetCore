namespace Anycode.NetCore.DatabaseTemplate.Entities;

public class ActiveUsersStatsEntity
{
	[Key]
	public Guid Id { get; init; }

	public required DateTimeOffset Date { get; init; }
	public required bool OnlyRegistered { get; init; }
	public required int DailyActiveUsers { get; init; }
	public required int WeeklyActiveUsers { get; init; }
	public required int MonthlyActiveUsers { get; init; }
}