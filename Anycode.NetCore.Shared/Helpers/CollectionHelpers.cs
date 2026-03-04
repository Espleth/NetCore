namespace Anycode.NetCore.Shared.Helpers;

public static class CollectionHelpers
{
	/// <summary>
	/// Paginate elementsCount into pages of maxPerQuery elements
	/// </summary>
	public static List<(int skip, int take)> Paginate(int elementsCount, int maxPerQuery)
	{
		var paginationParams = new List<(int skip, int take)>();

		var skip = 0;
		var remainingElements = elementsCount;

		while (remainingElements > 0)
		{
			var take = Math.Min(maxPerQuery, remainingElements);
			paginationParams.Add((skip, take));

			skip += take;
			remainingElements -= take;
		}

		return paginationParams;
	}

	public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> items)
	{
		foreach (var item in items)
		{
			hashSet.Add(item);
		}
	}

	public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>(this IEnumerable<TValue> values, Func<TValue, TKey> keySelector)
		where TKey : notnull
	{
		return new ConcurrentDictionary<TKey, TValue>(values.Select(x => new KeyValuePair<TKey, TValue>(keySelector(x), x)));
	}

	public static Dictionary<TKey, List<TValue>> GroupToDictionary<TKey, TValue>(this IEnumerable<TValue> values, Func<TValue, TKey> keySelector)
		where TKey : notnull
	{
		return values.GroupBy(keySelector).ToDictionary(x => x.Key, x => x.ToList());
	}

	public static void Increment<TKey>(this Dictionary<TKey, int> dict, TKey key) where TKey : notnull
	{
		var value = dict.GetValueOrDefault(key, 0);

		dict[key] = value + 1;
	}

	public static void AddToList<TKey, TValue>(this Dictionary<TKey, List<TValue>> dict, TKey key, TValue value) where TKey : notnull
	{
		if (!dict.TryGetValue(key, out var list))
		{
			list = new List<TValue>();
			dict[key] = list;
		}

		list.Add(value);
	}

	public static bool UnorderedEquals<T>(this IEnumerable<T> first, IEnumerable<T> second)
	{
		return first.OrderBy(t => t).SequenceEqual(second.OrderBy(t => t));
	}

	public static T FirstOr<T>(this ICollection<T> collection, T defaultValue)
	{
		return collection.Count > 0 ? collection.First() : defaultValue;
	}

	extension<T>(ICollection<T>? collection)
	{
		public bool IsAny()
		{
			return collection != null && collection.Count > 0;
		}

		public bool NotAny()
		{
			return collection == null || collection.Count == 0;
		}
	}

	public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(
		this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, SortDirection direction)
	{
		return direction == SortDirection.Ascending ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);
	}

	public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
		this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, SortDirection direction)
	{
		return direction == SortDirection.Ascending ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);
	}

	public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(
		this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, bool ascending)
	{
		return ascending ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);
	}

	public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
		this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, bool ascending)
	{
		return ascending ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);
	}
}