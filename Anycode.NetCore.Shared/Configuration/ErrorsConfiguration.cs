namespace Anycode.NetCore.Shared.Configuration;

public record ErrorsConfig
{
	public string UnexpectedErrorMessage { get; init; } = "An unexpected error occurred.";
}