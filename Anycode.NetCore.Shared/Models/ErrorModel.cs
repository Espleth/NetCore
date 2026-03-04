namespace Anycode.NetCore.Shared.Models;

/// <summary>
/// Based on RFC 7807, we return errors in this format using Problem Details
/// This record made just for OpenAPI documentation
/// </summary>
[PublicAPI]
public record ErrorModel(string Type, string Title, int Status, string Detail, string TraceId, int? ErrorCode, List<ErrorMessage>? AdditionalErrors);

[PublicAPI]
public record ErrorMessage(int ErrorCode, string Title);