namespace Anycode.NetCore.ApiTemplate.Endpoints.Test;

[PublicAPI]
public record TestResponse2
{
	[Range(0, 100)]
	public int Take { get; init; }
	public int Skip { get; init; }
}