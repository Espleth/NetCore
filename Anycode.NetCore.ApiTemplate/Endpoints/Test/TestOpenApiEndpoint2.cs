namespace Anycode.NetCore.ApiTemplate.Endpoints.Test;

public class TestOpenApiEndpoint2 : IEndpoint
{
	public void Register(IEndpointRouteBuilder endpointsBuilder)
	{
		endpointsBuilder.MapGet("/v1/test/call2", Handle)
			.ForceNoAuth()
			.WithSummary("Test2")
			.WithDescription("Example endpoint to demonstrate how different aspects will work with OpenAPI")
			.WithTags("Test");
	}

	private static TestResponse2 Handle([AsParameters] PagingQuery paging)
	{
		paging.Validate();
		return new TestResponse2
		{
			Skip = paging.Skip,
			Take = paging.Take,
		};
	}
}