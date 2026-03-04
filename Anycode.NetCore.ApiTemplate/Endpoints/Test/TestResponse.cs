namespace Anycode.NetCore.ApiTemplate.Endpoints.Test;

[PublicAPI]
public record TestResponse
{
	public required ErrorCode ExampleEnum { get; init; }
	public required DateTimeOffset ExampleDate { get; init; }
	public required Guid ExampleGuid { get; init; }
}