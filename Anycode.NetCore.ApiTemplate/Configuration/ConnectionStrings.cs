namespace Anycode.NetCore.ApiTemplate.Configuration;

public record ConnectionStrings
{
	public string? AppDb { get; init; }
	public string? Redis { get; init; }
	public string? RabbitMq { get; init; }
}