namespace Anycode.NetCore.Shared.Infrastructure.OpenApiTransformers;

/// <summary>
/// Adds Bearer security scheme to OpenAPI document if Bearer authentication is configured
/// </summary>
public class OpenApiBearerSecurityTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
	public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
	{
		var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
		if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
		{
			var requirements = new Dictionary<string, IOpenApiSecurityScheme>
			{
				["Bearer"] = new OpenApiSecurityScheme
				{
					Type = SecuritySchemeType.Http,
					Scheme = "bearer",
					In = ParameterLocation.Cookie,
					BearerFormat = "Json Web Token"
				}
			};
			document.Components ??= new OpenApiComponents();
			document.Components.SecuritySchemes = requirements;
		}
	}
}