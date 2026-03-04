namespace Anycode.NetCore.Shared.Infrastructure.Exceptions;

public class UnauthorizedApiException : ApiException
{
	public override ErrorInfo Error => new (401, "Unauthorized", "The request requires user authentication.");
}