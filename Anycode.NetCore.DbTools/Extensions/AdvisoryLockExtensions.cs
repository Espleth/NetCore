namespace Anycode.NetCore.DbTools.Extensions;

/// <summary>
/// PostgreSQL advisory lock extensions for <see cref="DatabaseFacade"/>.
/// Must be used within an active transaction. Locks are automatically released at the end of the transaction.
/// </summary>
public static class AdvisoryLockExtensions
{
	extension(DatabaseFacade database)
	{
		/// <summary>
		/// Acquires a transaction-scoped advisory lock on a single key.
		/// </summary>
		public async Task AcquireAdvisoryLockAsync(long key, CancellationToken ct = default)
		{
			await database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock({0})", [key], ct);
		}

		/// <summary>
		/// Acquires transaction-scoped advisory locks on multiple keys.
		/// </summary>
		public async Task AcquireAdvisoryLocksAsync(IEnumerable<long> keys, CancellationToken ct = default)
		{
			foreach (var key in keys.Order())
			{
				await database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock({0})", [key], ct);
			}
		}

		/// <summary>
		/// Tries to acquire a transaction-scoped advisory lock on a single key without blocking.
		/// Returns true if the lock was acquired, false if it's already held by another session.
		/// </summary>
		public async Task<bool> TryAcquireAdvisoryLockAsync(long key, CancellationToken ct = default)
		{
			// pg_try_advisory_xact_lock returns a single boolean column
			await using var command = database.GetDbConnection().CreateCommand();
			command.CommandText = "SELECT pg_try_advisory_xact_lock(@p0)";
			var param = command.CreateParameter();
			param.ParameterName = "p0";
			param.Value = key;
			command.Parameters.Add(param);

			if (command.Connection!.State != System.Data.ConnectionState.Open)
				await command.Connection.OpenAsync(ct);

			var result = await command.ExecuteScalarAsync(ct);
			return result is true;
		}

		/// <summary>
		/// Tries to acquire transaction-scoped advisory locks on multiple keys without blocking.
		/// Returns true only if ALL locks were acquired.
		/// If any lock fails, previously acquired locks remain held (they will be released when the transaction ends).
		/// </summary>
		public async Task<bool> TryAcquireAdvisoryLocksAsync(IEnumerable<long> keys, CancellationToken ct = default)
		{
			foreach (var key in keys.Order())
			{
				if (!await database.TryAcquireAdvisoryLockAsync(key, ct))
					return false;
			}

			return true;
		}
	}
}