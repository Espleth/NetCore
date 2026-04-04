namespace Anycode.NetCore.Shared.Helpers;

public static class QueryExtensions
{
	public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable) where T : struct
	{
		return enumerable.Where(x => x != null).Select(x => x!.Value);
	}

	public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable)
	{
		return enumerable.Where(x => x != null).Select(x => x!);
	}

	public static IQueryable<T> Paging<T>(this IQueryable<T> query, PagingQuery paging)
	{
		return query.Skip(paging.Skip).Take(paging.Take);
	}

	public static IEnumerable<T> Paging<T>(this IEnumerable<T> query, PagingQuery paging)
	{
		return query.Skip(paging.Skip).Take(paging.Take);
	}

	public static async Task<QueryableList<T>> ToQueryableListAsync<T>(this IQueryable<T> query,
		PagingQuery paging, CancellationToken ct)
	{
		var total = await query.CountAsync(ct);
		if (total == 0)
			return QueryableList<T>.Empty();

		var items = await query.Paging(paging).ToListAsync(ct);

		return new QueryableList<T>
		{
			Data = items,
			Total = total,
		};
	}
}