namespace Anycode.NetCore.Shared.Helpers;

public class AsyncLocker(int count = 1)
{
	private readonly SemaphoreSlim _semaphoreSlim = new(count, count);

	public async Task LockAsync(Func<Task> func)
	{
		await _semaphoreSlim.WaitAsync();

		try
		{
			await func();
		}
		finally
		{
			_semaphoreSlim.Release();
		}
	}

	public async Task<IDisposable> EnterAsync(CancellationToken cancellation = default)
	{
		await _semaphoreSlim.WaitAsync(cancellation);
		return new LockerRelease(_semaphoreSlim);
	}

	public async Task<T> LockAsync<T>(Func<Task<T>> func)
	{
		await _semaphoreSlim.WaitAsync();

		try
		{
			return await func();
		}
		finally
		{
			_semaphoreSlim.Release();
		}
	}

	private class LockerRelease(SemaphoreSlim semaphore) : IDisposable
	{
		private bool _disposed;

		public void Dispose()
		{
			if (_disposed)
				return;

			semaphore.Release();
			_disposed = true;
		}
	}
}