namespace Anycode.NetCore.ApiTemplate.Endpoints.Test;

[PublicAPI]
public record TestRequest
{
	public required DateTimeOffset SomeDate { get; init; }
	public required Guid SomeGuid { get; init; }
}