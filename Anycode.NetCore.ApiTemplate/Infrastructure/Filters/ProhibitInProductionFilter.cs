namespace Anycode.NetCore.ApiTemplate.Infrastructure.Filters;

public class ProhibitInProductionFilter(EnvironmentConfig envConfig) : IEndpointFilter
{
	public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
	{
		var endpoint = context.HttpContext.GetEndpoint();
		if (endpoint == null)
			return next(context);

		if (envConfig.Environment != EnvironmentType.Production)
			return next(context);

		var metadata = endpoint.Metadata.GetMetadata<ProhibitInProductionMetadata>();
		if (metadata == null)
			return next(context);

		context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
		return next(context);
	}
}