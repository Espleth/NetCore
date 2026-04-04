namespace Anycode.NetCore.ApiTemplate.Infrastructure.Extensions;

public static class EndpointsHelper
{
	public static IEndpointConventionBuilder WithErrorsDescription(this IEndpointConventionBuilder builder,
		string description, params (ErrorCode error, string errorInfo)[] errors)
	{
		return builder.WithDescription($"{description} {MakeErrorsDescription(errors)}");
	}

	public static IEndpointConventionBuilder WithErrorsDescription(this IEndpointConventionBuilder builder,
		string description, params ErrorCode[] errors)
	{
		return builder.WithDescription($"{description} {MakeErrorsDescription(errors)}");
	}

	public static IEndpointConventionBuilder WithErrorsDescription(this IEndpointConventionBuilder builder,
		params (ErrorCode error, string errorInfo)[] errors)
	{
		return builder.WithDescription(MakeErrorsDescription(errors));
	}

	public static IEndpointConventionBuilder WithErrorsDescription(this IEndpointConventionBuilder builder,
		params ErrorCode[] errors)
	{
		return builder.WithDescription(MakeErrorsDescription(errors));
	}

	private static string MakeErrorsDescription(params ErrorCode[] errors)
	{
		return MakeErrorsDescription(errors.Select(x => (x, "")).ToArray());
	}

	private static string MakeErrorsDescription(params (ErrorCode error, string errorInfo)[] errors)
	{
		if (!errors.Any())
			return "";

		return $"Errors: {string.Join("; ", errors.Select(x => string.IsNullOrEmpty(x.errorInfo)
			? $"{x.error}({(int)x.error})"
			: $"{x.error}({(int)x.error}): {x.errorInfo}"))}";
	}
}