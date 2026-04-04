#pragma warning disable CA2254

namespace Anycode.NetCore.Shared.Helpers;

public static class LoggingExtensions
{
	// Limit content length per argument to 100k chars for logging, seq can handle only up to 262144 bytes
	// This will work only per argument and only if the argument is a string, so this might still not work for some cases
	private const int MaxArgumentLength = 100000;

	// ReSharper disable once ConvertToExtensionBlock
	// For whatever reason, when converting to extension block, log code start to produce CS8620 warning on build

	public static void Trace(this ILogger log, [StructuredMessageTemplate] string? message, params object?[] args)
	{
		log.LogInternal(LogLevel.Trace, LogLevel.Trace, MaxArgumentLength, null, message, args);
	}

	public static void Trace(this ILogger log,
		Exception exception, [StructuredMessageTemplate] string? message, params object?[] args)
	{
		log.LogInternal(LogLevel.Trace, LogLevel.Trace, MaxArgumentLength, exception, message, args);
	}

	public static void Debug(this ILogger log, [StructuredMessageTemplate] string? message, params object?[] args)
	{
		log.LogInternal(LogLevel.Debug, LogLevel.Trace, MaxArgumentLength, null, message, args);
	}

	public static void Debug(this ILogger log,
		Exception exception, [StructuredMessageTemplate] string? message, params object?[] args)
	{
		log.LogInternal(LogLevel.Debug, LogLevel.Trace, MaxArgumentLength, exception, message, args);
	}

	public static void Info(this ILogger log, [StructuredMessageTemplate] string? message, params object?[] args)
	{
		log.LogInternal(LogLevel.Information, LogLevel.Trace, MaxArgumentLength, null, message, args);
	}

	public static void Info(this ILogger log,
		Exception exception, [StructuredMessageTemplate] string? message, params object?[] args)
	{
		log.LogInternal(LogLevel.Information, LogLevel.Trace, MaxArgumentLength, exception, message, args);
	}

	public static void Warn(this ILogger log, [StructuredMessageTemplate] string? message, params object?[] args)
	{
		log.LogInternal(LogLevel.Warning, LogLevel.Trace, MaxArgumentLength, null, message, args);
	}

	public static void Warn(this ILogger log,
		Exception exception, [StructuredMessageTemplate] string? message, params object?[] args)
	{
		log.LogInternal(LogLevel.Warning, LogLevel.Trace, MaxArgumentLength, exception, message, args);
	}

	public static void Error(this ILogger log, [StructuredMessageTemplate] string? message, params object?[] args)
	{
		log.LogInternal(LogLevel.Error, LogLevel.Trace, MaxArgumentLength, null, message, args);
	}

	public static void Error(this ILogger log,
		Exception exception, [StructuredMessageTemplate] string? message, params object?[] args)
	{
		log.LogInternal(LogLevel.Error, LogLevel.Trace, MaxArgumentLength, exception, message, args);
	}

	public static void Fatal(this ILogger log, [StructuredMessageTemplate] string? message, params object?[] args)
	{
		log.LogInternal(LogLevel.Critical, LogLevel.Trace, MaxArgumentLength, null, message, args);
	}

	public static void Fatal(this ILogger log,
		Exception exception, [StructuredMessageTemplate] string? message, params object?[] args)
	{
		log.LogInternal(LogLevel.Critical, LogLevel.Trace, MaxArgumentLength, exception, message, args);
	}

	public static void Log(this ILogger log,
		LogLevel logLevel, LogLevel minLevel, [StructuredMessageTemplate] string? message, params object?[] args)
	{
		log.LogInternal(logLevel, minLevel, MaxArgumentLength, null, message, args);
	}

	public static void Log(this ILogger log, LogLevel logLevel, LogLevel minLevel, int maxArgLength,
		[StructuredMessageTemplate] string? message, params object?[] args)
	{
		log.LogInternal(logLevel, minLevel, maxArgLength, null, message, args);
	}

	public static void Log(this ILogger log, LogLevel logLevel, LogLevel minLevel, Exception? exception,
		[StructuredMessageTemplate] string? message, params object?[] args)
	{
		log.LogInternal(logLevel, minLevel, MaxArgumentLength, exception, message, args);
	}

	private static void LogInternal(this ILogger log,
		LogLevel logLevel, LogLevel minLevel, int maxArgLength, Exception? exception,
		[StructuredMessageTemplate] string? message, params object?[] args)
	{
		if (logLevel < minLevel)
			return;

		for (var i = 0; i < args.Length; i++)
		{
			if (args[i] is string str)
				args[i] = Truncate(str, maxArgLength);
		}

		log.Log(logLevel, exception, message, args);
	}

	public static string? TruncateForLog(this string? str)
	{
		return Truncate(str, MaxArgumentLength);
	}

	private static string? Truncate(string? str, int maxLength)
	{
		if (str?.Length > maxLength)
			str = str[..maxLength] + "... [content trimmed]";
		return str;
	}
}