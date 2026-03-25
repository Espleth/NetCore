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
				await WriteProblemDetailsAsync(context, exception);
			}
			else if (exception is BadHttpRequestException)
			{
				context.Response.StatusCode = StatusCodes.Status400BadRequest;
				context.Features.Set(new LogErrorFeature(nameof(BadHttpRequestException)));
				await WriteProblemDetailsAsync(context, exception, "BadRequest", exception.Message);
			}
			else if (exception is ApiException || exception.InnerException is ApiException) // Inner mostly for wrapped EF Core exceptions
			{
				var endpointException = exception as ApiException ?? exception.InnerException as ApiException;

				context.Features.Set(new LogErrorFeature(string.Join(", ", endpointException!.AdditionalErrors.Select(x => x.ToString()))));
				context.Response.StatusCode = endpointException.HttpCode;
				await WriteProblemDetailsAsync(context, exception, endpointException.Error.CodeName, endpointException.Error.Message,
					new Dictionary<string, object?>
					{
						{ "errorCode", endpointException.Error.Code },
						{ "additionalErrors", endpointException.AdditionalErrors.Select(x => new ErrorMessage(x.Code, x.CodeName)).ToList() },
					});
			}
			else
			{
				log.Error(exception, "Exception executing user request");

				context.Features.Set(new LogErrorFeature($"Unexpected exception: {exception.GetType()}"));
				context.Response.StatusCode = StatusCodes.Status500InternalServerError;
				await WriteProblemDetailsAsync(context, exception, "ServerError",
					errorsConfiguration?.UnexpectedErrorMessage ?? "An unexpected error occurred.");
			}
		}
	}

	private async Task WriteProblemDetailsAsync(
		HttpContext context,
		Exception exception,
		string? title = null,
		string? detail = null,
		Dictionary<string, object?>? extensions = null)
	{
		var problemDetails = new ProblemDetails
		{
			Status = context.Response.StatusCode,
			Title = title,
			Detail = detail,
		};

		if (extensions != null)
		{
			foreach (var (key, value) in extensions)
				problemDetails.Extensions[key] = value;
		}

		var problemDetailsContext = new ProblemDetailsContext
		{
			HttpContext = context,
			Exception = exception,
			ProblemDetails = problemDetails,
		};

		if (!await problemDetailsService.TryWriteAsync(problemDetailsContext))
		{
			context.Response.ContentType = "application/problem+json";
			await context.Response.WriteAsJsonAsync(problemDetails, context.RequestAborted);
		}
	}
}