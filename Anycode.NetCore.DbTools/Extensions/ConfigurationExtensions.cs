namespace Anycode.NetCore.DbTools.Extensions;

public static class ConfigurationExtensions
{
	extension(DbSet<ConfigurationEntity> dbSet)
	{
		public async Task<string?> GetAsync(string key, CancellationToken ct = default)
		{
			var res = await dbSet.FirstOrDefaultAsync(x => x.Key == key, ct);
			return res?.Value;
		}

		public async Task<DateTimeOffset?> GetDtAsync(string key, CancellationToken ct = default)
		{
			var res = await dbSet.GetAsync(key, ct);
			if (res == null)
				return null;

			return DateTimeOffset.TryParse(res, out var result) ? result : null;
		}

		public async Task<int?> GetIntAsync(string key, CancellationToken ct = default)
		{
			var res = await dbSet.GetAsync(key, ct);
			if (res == null)
				return null;

			return int.TryParse(res, out var result) ? result : null;
		}

		public async Task<long?> GetLongAsync(string key, CancellationToken ct = default)
		{
			var res = await dbSet.GetAsync(key, ct);
			if (res == null)
				return null;

			return long.TryParse(res, out var result) ? result : null;
		}

		public async Task SetAsync(string key, string? value)
		{
			var currentValue = await dbSet.AsTracking().FirstOrDefaultAsync(x => x.Key == key);
			if (currentValue != null)
			{
				currentValue.Value = value;
				return;
			}

			var config = new ConfigurationEntity
			{
				Key = key,
				Value = value,
			};
			dbSet.Add(config);
		}

		public Task SetAsync(string key, DateTimeOffset? value)
		{
			return dbSet.SetAsync(key, value?.ToString("O"));
		}

		public Task SetAsync(string key, int? value)
		{
			return dbSet.SetAsync(key, value?.ToString());
		}

		public Task SetAsync(string key, long? value)
		{
			return dbSet.SetAsync(key, value?.ToString());
		}
	}
}