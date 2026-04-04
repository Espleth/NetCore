namespace Anycode.NetCore.Shared.Infrastructure.OpenApiTransformers;

/// <summary>
/// Add current DateTime to all date-time parameters
/// </summary>
public class OpenApiDateTimeTransformer : IOpenApiSchemaTransformer
{
	public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context,
		CancellationToken cancellationToken)
	{
		if (schema.Format != "date-time")
			return Task.CompletedTask;

		schema.Description = "ISO 8601 time";
		schema.Example = DateTimeOffset.UtcNow.ToUtcIsoWithoutMs();
		return Task.CompletedTask;
	}
}