namespace Anycode.NetCore.HealthChecker;

public static class HealthCheckerConstants
{
	/// <summary>
	/// For health checkers to check each other
	/// </summary>
	public static string HealthCheckerLastCheckDate(string name) => $"{name}_HealthCheckerLastCheckDate";

	public const string ConfigTableName = "Configuration";
}