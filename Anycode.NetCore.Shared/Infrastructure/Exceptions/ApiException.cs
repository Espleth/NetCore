namespace Anycode.NetCore.Shared.Infrastructure.Exceptions;

public abstract class ApiException : Exception
{
	public virtual int HttpCode => Error.Code;
	public abstract ErrorInfo Error { get; }
	public virtual List<ErrorInfo> AdditionalErrors { get; } = [];
}

public record ErrorInfo(int Code, string CodeName, string Message);