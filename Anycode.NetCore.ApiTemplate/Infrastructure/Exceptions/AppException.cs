namespace Anycode.NetCore.ApiTemplate.Infrastructure.Exceptions;

public class AppException : ApiException
{
	public static AppException BadRequest => new(ErrorCode.BadRequest);
	public static AppException Unauthorized => new(ErrorCode.Unauthorized);
	public static AppException Forbidden => new(ErrorCode.Forbidden);
	public static AppException NotFound => new(ErrorCode.NotFound);

	public override int HttpCode => GetStatusCode();
	public override ErrorInfo Error => new((int)ErrorCode, ErrorCode.ToString(), ErrorMessage);
	public override List<ErrorInfo> AdditionalErrors =>
		AdditionalErrorCodes.Select(x => new ErrorInfo((int)x, x.ToString(), x.GetName())).ToList();

	private ErrorCode ErrorCode { get; }
	private string ErrorMessage { get; }
	private List<ErrorCode> AdditionalErrorCodes { get; } = [];

	public AppException(ErrorCode code) : this(code, code.GetName())
	{
	}

	public AppException(ErrorCode code, string message)
	{
		ErrorCode = code;
		ErrorMessage = message;
	}

	public AppException(ICollection<ErrorCode> codes) : this(codes, codes.First().GetName())
	{
	}

	public AppException(ICollection<ErrorCode> codes, string message)
	{
		if (!codes.Any())
			throw new ArgumentException("At least one error code must be provided", nameof(codes));

		ErrorCode = codes.First();
		ErrorMessage = message;
		AdditionalErrorCodes.AddRange(codes.Skip(1));
	}

	private int GetStatusCode()
	{
		return ErrorCode switch
		{
			ErrorCode.Unauthorized => StatusCodes.Status401Unauthorized,
			ErrorCode.Forbidden => StatusCodes.Status403Forbidden,
			ErrorCode.NotFound => StatusCodes.Status404NotFound,
			ErrorCode.RateLimit => StatusCodes.Status429TooManyRequests,
			_ => StatusCodes.Status400BadRequest
		};
	}
}