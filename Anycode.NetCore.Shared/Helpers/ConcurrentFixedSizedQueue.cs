namespace Anycode.NetCore.Shared.Helpers;

/// <summary>
/// Maybe not the best solution (not concurrent), but it'll do for now.
/// </summary>
public class ConcurrentFixedSizedQueue<T>(int size) : IReadOnlyCollection<T>
{
	public int Size => size;
	public int Count => _queue.Count;

	private readonly ConcurrentQueue<T> _queue = new();

	public void Enqueue(T obj)
	{
		_queue.Enqueue(obj);

		while (_queue.Count > Size)
		{
			_queue.TryDequeue(out _);
		}
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _queue.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}