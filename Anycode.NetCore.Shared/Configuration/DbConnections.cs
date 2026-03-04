namespace Anycode.NetCore.Shared.Configuration;

public class DbConnections
{
	public string? AppDb { get; init; }
	public string? Redis { get; init; }
	public string? RabbitMqUrl { get; init; }
}