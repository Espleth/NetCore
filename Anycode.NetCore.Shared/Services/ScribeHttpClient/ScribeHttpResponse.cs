using RestSharp;

namespace Anycode.NetCore.Shared.Services.ScribeHttpClient;

public class ScribeHttpResponse
{
	public bool IsSuccessful { get; init; }
	public HttpStatusCode StatusCode { get; init; }
	public string? ErrorMessage { get; init; }
	public string? Content { get; init; }
	public Exception? ErrorException { get; init; }

	public ScribeHttpResponse(bool isSuccessful, HttpStatusCode statusCode = HttpStatusCode.OK)
	{
		IsSuccessful = isSuccessful;
		StatusCode = statusCode;
	}

	public ScribeHttpResponse(Exception exception)
	{
		ErrorMessage = $"Exception occurred while executing request: {exception.Message}";
		ErrorException = exception;
	}

	public ScribeHttpResponse(RestResponse response)
	{
		IsSuccessful = response.IsSuccessful;
		StatusCode = response.StatusCode;
		ErrorMessage = response.ErrorMessage;
		Content = response.Content;
		ErrorException = response.ErrorException;
	}
}

public class ScribeHttpResponse<T> : ScribeHttpResponse
{
	public T? Data { get; init; }

	public ScribeHttpResponse(T data) : base(true)
	{
		Data = data;
	}

	public ScribeHttpResponse(Exception exception) : base(exception) { }

	public ScribeHttpResponse(RestResponse<T> response) : base(response)
	{
		IsSuccessful = response.IsSuccessful && response.Data != null;
		Data = response.Data;
	}
}