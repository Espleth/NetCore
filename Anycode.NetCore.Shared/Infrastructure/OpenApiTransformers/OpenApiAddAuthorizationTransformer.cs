namespace Anycode.NetCore.Shared.Infrastructure.OpenApiTransformers;

/// <summary>
/// Show auth as required if no ForceNoAuth or AllowAnonymous attribute is present
/// </summary>
public class OpenApiAddAuthorizationTransformer : IOpenApiOperationTransformer
{
	public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken ct)
	{
		var actionMetadata = context.Description.ActionDescriptor.EndpointMetadata;
		// Ideally allow anonymous should be still show scheme, but scalar will display that auth is required in this case
		if (actionMetadata.Any(x => x is ForceNoAuthMetadata) || actionMetadata.Any(x => x is AllowAnonymousAttribute))
		{
			operation.Security = [];
			return Task.CompletedTask;
		}

		operation.Parameters ??= new List<IOpenApiParameter>();
		operation.Security = new List<OpenApiSecurityRequirement>();

		//Add JWT bearer type
		operation.Security.Add(new OpenApiSecurityRequirement
		{
			{
				new OpenApiSecuritySchemeReference("Bearer", new OpenApiDocument()),
				[]
			}
		});
		return Task.CompletedTask;
	}
}