namespace Anycode.NetCore.Shared.Infrastructure.Middlewares;

public class ExceptionHandlingMiddleware(
	RequestDelegate next,
	IProblemDetailsService problemDetailsService,
	ILogger<ExceptionHandlingMiddleware> log,
	ErrorsConfig? errorsConfiguration = null)
{
	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await next(context);
		}
		catch (Exception exception)
		{
			if (exception is OperationCanceledException)
			{
				context.Response.StatusCode = StatusCodes.Status400BadRequest;
				context.Features.Set(new LogErrorFeature(nameof(OperationCanceledException)));
				await problemDetailsService.WriteAsync(new ProblemDetailsContext
				{
					HttpContext = context,
					Exception = exception,
				});
			}
			else if (exception is BadHttpRequestException)
			{
				context.Response.StatusCode = StatusCodes.Status400BadRequest;
				context.Features.Set(new LogErrorFeature(nameof(BadHttpRequestException)));
				await problemDetailsService.WriteAsync(new ProblemDetailsContext
				{
					HttpContext = context,
					Exception = exception,
					ProblemDetails =
					{
						Title = "BadRequest",
						Detail = exception.Message,
					}
				});
			}
			else if (exception is ApiException || exception.InnerException is ApiException) // Inner mostly for wrapped EF Core exceptions
			{
				var endpointException = exception as ApiException ?? exception.InnerException as ApiException;

				context.Features.Set(new LogErrorFeature(string.Join(", ", endpointException!.AdditionalErrors.Select(x => x.ToString()))));
				context.Response.StatusCode = endpointException.HttpCode;
				await problemDetailsService.WriteAsync(new ProblemDetailsContext
				{
					HttpContext = context,
					Exception = exception,
					ProblemDetails =
					{
						Title = endpointException.Error.CodeName,
						Detail = endpointException.Error.Message,
						Extensions = new Dictionary<string, object?>
						{
							{ "errorCode", endpointException.Error.Code },
							{ "additionalErrors", endpointException.AdditionalErrors.Select(x => new ErrorMessage(x.Code, x.CodeName)).ToList() },
						}
					},
				});
			}
			else
			{
				log.Error(exception, "Exception executing user request");

				context.Features.Set(new LogErrorFeature($"Unexpected exception: {exception.GetType()}"));
				context.Response.StatusCode = StatusCodes.Status500InternalServerError;
				await problemDetailsService.WriteAsync(new ProblemDetailsContext
				{
					HttpContext = context,
					Exception = exception,
					ProblemDetails =
					{
						Title = "ServerError",
						Detail = errorsConfiguration?.UnexpectedErrorMessage ?? "An unexpected error occurred.",
					},
				});
			}
		}
	}
}