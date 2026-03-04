namespace Anycode.NetCore.Shared.Infrastructure.OpenApiTransformers;

/// <summary>
/// Read headers from SwaggerShowHeadersAttribute and add them to the Swagger documentation
/// </summary>
public class OpenApiShowHeadersTransformer : IOpenApiOperationTransformer
{
	public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken ct)
	{
		operation.Parameters ??= new List<IOpenApiParameter>();

		// For controllers
		var headersData = context.Description.ActionDescriptor.EndpointMetadata.OfType<OpenApiShowHeadersMetadata>().FirstOrDefault();
		if (headersData == null)
			return Task.CompletedTask;

		foreach (var header in headersData.Headers)
		{
			operation.Parameters.Add(new OpenApiParameter
			{
				Name = header,
				In = ParameterLocation.Header,
				Required = false,
				Schema = GetHeaderSchema(header),
			});
		}

		return Task.CompletedTask;
	}

	private static OpenApiSchema GetHeaderSchema(string header)
	{
		return header switch
		{
			_ => new OpenApiSchema { Type = JsonSchemaType.String }
		};
	}
}