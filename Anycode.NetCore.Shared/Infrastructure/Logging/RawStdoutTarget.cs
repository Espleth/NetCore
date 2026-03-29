using NLog;
using NLog.Targets;

namespace Anycode.NetCore.Shared.Infrastructure.Logging;

/// <summary>
/// For Aspire
/// </summary>
[Target("RawStdout")]
public sealed class RawStdoutTarget : TargetWithLayout
{
	private static readonly Lock _sync = new();
	private static readonly Stream _output = Console.OpenStandardOutput();
	private static readonly UTF8Encoding _encoding = new(false);

	protected override void Write(LogEventInfo logEvent)
	{
		var rendered = RenderLogEvent(Layout, logEvent);
		var bytes = _encoding.GetBytes(rendered + Environment.NewLine);

		lock (_sync)
		{
			_output.Write(bytes, 0, bytes.Length);
			_output.Flush();
		}
	}
}

