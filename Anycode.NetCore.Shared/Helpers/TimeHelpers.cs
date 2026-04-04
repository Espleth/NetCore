namespace Anycode.NetCore.Shared.Helpers;

public static class TimeHelpers
{
	extension(DateTimeOffset)
	{
		public static DateTimeOffset UtcCurrentMonth
			=> new(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);
	}

	extension(DateTimeOffset dto)
	{
		public string ToUtcIsoWithoutMs()
		{
			return dto.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
		}

		public DateTimeOffset MonthStart()
		{
			return new DateTimeOffset(dto.Year, dto.Month, 1, 0, 0, 0, dto.Offset);
		}

		public DateTimeOffset ToMinuteStart()
		{
			return new DateTimeOffset(dto.Year, dto.Month, dto.Day, dto.Hour, dto.Minute, 0, dto.Offset);
		}

		public DateTimeOffset ToDayStart()
		{
			return new DateTimeOffset(dto.Year, dto.Month, dto.Day, 0, 0, 0, dto.Offset);
		}

		public DateOnly ToDateOnly()
		{
			return DateOnly.FromDateTime(dto.Date);
		}

		public TimeOnly ToTimeOnly()
		{
			return TimeOnly.FromDateTime(dto.UtcDateTime);
		}

		public DateTimeOffset Date()
		{
			return new DateTimeOffset(dto.Year, dto.Month, dto.Day, 0, 0, 0, TimeSpan.Zero);
		}

		public DateTimeOffset ToDateTimeOffset(TimeOnly time)
		{
			return new DateTimeOffset(dto.Year, dto.Month, dto.Day, time.Hour, time.Minute, time.Second, dto.Offset);
		}
	}

	extension(DateOnly date)
	{
		public DateTimeOffset ToDateTimeOffset()
		{
			return new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, 0, TimeSpan.Zero);
		}

		public DateTimeOffset ToDateTimeOffset(TimeOnly time)
		{
			return new DateTimeOffset(date, time, TimeSpan.Zero);
		}

		public DateTime ToDateTime()
		{
			return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
		}
	}

	extension(DateOnly)
	{
		public static DateOnly UtcToday => DateOnly.FromDateTime(DateTime.UtcNow.Date);
		public static DateOnly UtcTomorrow => DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1));
	}

	/// <summary>
	/// Check if time is in the range of startTime and endTime.
	/// If startTime > endTime, endTime will be treated as the next day time (e.g. from 20:00 to 8:00 the next day)
	/// If startTime == null, it will be treated as 00:00
	/// If endTime == null, it will be treated as 00:00 the next day
	/// </summary>
	public static bool IsInTimeRange(this TimeOnly time, TimeOnly? startTime, TimeOnly? endTime)
	{
		if (startTime != null && endTime != null)
		{
			if (endTime > startTime)
				return time >= startTime.Value && time < endTime.Value;
			return time >= startTime.Value || time < endTime.Value;
		}

		if (startTime != null)
			return time >= startTime.Value;
		if (endTime != null)
			return time < endTime.Value;

		return true;
	}
}