namespace Anycode.NetCore.Shared.Infrastructure.Extensions;

public static class EndpointsHelper
{
	public static IEndpointRouteBuilder MapGroupWithTag(this IEndpointRouteBuilder endpointsBuilder, string tag, string routePrefix = "")
	{
		return endpointsBuilder.MapGroup(routePrefix).WithTags(tag);
	}

	public static IEndpointRouteBuilder Register<T>(this IEndpointRouteBuilder endpointsBuilder)
		where T : IEndpoint, new()
	{
		var endpointsProvider = new T();
		endpointsProvider.Register(endpointsBuilder);
		return endpointsBuilder;
	}

	/// <summary>
	/// What headers to display in OpenAPI for this endpoint
	/// </summary>
	public static TBuilder WithHeaders<TBuilder>(this TBuilder builder, params string[] headers) where TBuilder : IEndpointConventionBuilder
	{
		return builder.WithMetadata(new OpenApiShowHeadersMetadata(headers));
	}

	/// <summary>
	/// Force no authentication for this endpoint
	/// </summary>
	public static TBuilder ForceNoAuth<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
	{
		return builder.AllowAnonymous().WithMetadata(new ForceNoAuthMetadata());
	}
}