namespace Anycode.NetCore.Shared.Infrastructure.Metadata;

public class OpenApiShowHeadersMetadata(params string[] headers)
{
	public string[] Headers => headers;
}