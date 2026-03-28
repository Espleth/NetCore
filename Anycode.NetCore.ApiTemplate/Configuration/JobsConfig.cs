namespace Anycode.NetCore.ApiTemplate.Configuration;

public record JobsConfig
{
	public bool RunJobs { get; init; }
	public string? LastActivitiesCron { get; init; } = "0 5 * ? * *"; // Every hour at minute 5
}