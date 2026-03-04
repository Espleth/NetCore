using Scalar.AspNetCore;

namespace Anycode.NetCore.ApiTemplate.Endpoints.Test;

public class TestOpenApiEndpoint : IEndpoint
{
	public void Register(IEndpointRouteBuilder endpointsBuilder)
	{
		endpointsBuilder.MapPost("/v1/test/call", HandleAsync)
			.ForceNoAuth()
			.WithSummary("Test")
			.WithDescription("Example endpoint to demonstrate how different aspects will work with OpenAPI")
			.WithBadge("TestBadge", BadgePosition.Before, color: "#f3f30c")
			.WithTags("Test");
	}

	private static Task<TestResponse> HandleAsync(ErrorCode exampleEnumQuery, DateTimeOffset exampleDateQuery, Guid exampleGuidQuery, TestRequest body)
	{
		return Task.FromResult(new TestResponse
		{
			ExampleEnum = exampleEnumQuery,
			ExampleDate = exampleDateQuery,
			ExampleGuid = body.SomeGuid,
		});
	}
}