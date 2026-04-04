namespace Anycode.NetCore.Shared.Infrastructure.Extensions;

public static class EndpointsHelper
{
	extension(IEndpointRouteBuilder endpointsBuilder)
	{
		public IEndpointRouteBuilder MapGroupWithTag(string tag, string routePrefix = "")
		{
			return endpointsBuilder.MapGroup(routePrefix).WithTags(tag);
		}

		public IEndpointRouteBuilder Register<T>() where T : IEndpoint, new()
		{
			var endpointsProvider = new T();
			endpointsProvider.Register(endpointsBuilder);
			return endpointsBuilder;
		}
	}

	extension<TBuilder>(TBuilder builder) where TBuilder : IEndpointConventionBuilder
	{
		/// <summary>
		/// What headers to display in OpenAPI for this endpoint
		/// </summary>
		public TBuilder WithHeaders(params string[] headers)
		{
			return builder.WithMetadata(new OpenApiShowHeadersMetadata(headers));
		}

		/// <summary>
		/// Force no authentication for this endpoint
		/// </summary>
		public TBuilder ForceNoAuth()
		{
			return builder.AllowAnonymous().WithMetadata(new ForceNoAuthMetadata());
		}
	}
}