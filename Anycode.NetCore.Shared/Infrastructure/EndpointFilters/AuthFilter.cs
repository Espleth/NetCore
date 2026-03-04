namespace Anycode.NetCore.Shared.Infrastructure.EndpointFilters;

public class AuthFilter<TId>(IUserStampCacheService<TId> usersStampsCache, ILogger<AuthFilter<TId>> log) : IEndpointFilter where TId : struct
{
	private const string TypeStamp = "jti";

	public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
	{
		var endpoint = context.HttpContext.GetEndpoint();
		if (endpoint == null)
			return await next(context);

		if (endpoint.Metadata.GetMetadata<AllowAnonymousAttribute>() != null)
			return await next(context);

		if (context.HttpContext.User.Identity?.IsAuthenticated == true && !string.IsNullOrEmpty(context.HttpContext.User.Identity?.Name))
		{
			var stamp = context.HttpContext.User.Identities.FirstOrDefault()?.Claims.FirstOrDefault(x => x.Type == TypeStamp)?.Value;
			var id = context.HttpContext.User.Identity?.Name;

			if (!SystemHelpers.TryParse<TId>(id, out var userId))
			{
				log.Error("Could not parse user id from JWT {UserId}", id);
				context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
				return await next(context);
			}

			try
			{
				var currStamp = await usersStampsCache.GetUserStampByIdAsync(userId, CancellationToken.None);
				if (currStamp.IsBlocked)
				{
					context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
				}
				else if (!currStamp.HasPassword || currStamp.SecurityStamp != stamp)
				{
					context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
				}
			}
			catch (ApiException e)
			{
				if (e.HttpCode != StatusCodes.Status401Unauthorized)
					throw;

				context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
			}
		}

		return await next(context);
	}
}