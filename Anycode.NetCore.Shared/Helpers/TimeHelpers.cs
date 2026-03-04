namespace Anycode.NetCore.Shared.Helpers;

public static class TimeHelpers
{
	extension(DateTimeOffset)
	{
		public static DateTimeOffset UtcCurrentMonth => new(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);
	}

	extension(DateTimeOffset dateTimeOffset)
	{
		public string ToUtcIsoWithoutMs()
		{
			return dateTimeOffset.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
		}

		public DateTimeOffset MonthStart()
		{
			return new DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, 1, 0, 0, 0, dateTimeOffset.Offset);
		}

		public DateTimeOffset ToMinuteStart()
		{
			return new DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, dateTimeOffset.Hour, dateTimeOffset.Minute, 0,
				dateTimeOffset.Offset);
		}

		public DateTimeOffset ToDayStart()
		{
			return new DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, 0, 0, 0, dateTimeOffset.Offset);
		}

		public DateOnly ToDateOnly()
		{
			return DateOnly.FromDateTime(dateTimeOffset.Date);
		}

		public TimeOnly ToTimeOnly()
		{
			return TimeOnly.FromDateTime(dateTimeOffset.UtcDateTime);
		}

		public DateTimeOffset Date()
		{
			return new DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, 0, 0, 0, TimeSpan.Zero);
		}

		public DateTimeOffset ToDateTimeOffset(TimeOnly time)
		{
			return new DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, time.Hour, time.Minute, time.Second, dateTimeOffset.Offset);
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