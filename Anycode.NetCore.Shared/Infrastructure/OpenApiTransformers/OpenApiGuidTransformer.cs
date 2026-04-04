namespace Anycode.NetCore.Shared.Infrastructure.OpenApiTransformers;

/// <summary>
/// Prettify GUIDs
/// </summary>
public class OpenApiGuidTransformer : IOpenApiSchemaTransformer
{
	public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context,
		CancellationToken cancellationToken)
	{
		if (schema.Format != "uuid")
			return Task.CompletedTask;

		schema.Example = Guid.Empty.ToString();
		return Task.CompletedTask;
	}
}