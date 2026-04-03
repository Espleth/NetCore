namespace Anycode.NetCore.ApiTemplate.Infrastructure.Extensions;

public static class AppExtensions
{
	public static void Validate(this PagingQuery paging, int maxTake = DefaultValidation.MaxTake)
	{
		if (paging.Take < 0 || paging.Skip < 0)
		{
			throw new AppException(ErrorCode.InvalidPaging);
		}

		if (paging.Take > maxTake)
		{
			throw new AppException(ErrorCode.TooLargeTake);
		}
	}

	public static void ThrowIfAny(this ICollection<ErrorCode> errors)
	{
		if (errors.Count != 0)
		{
			throw new AppException(errors);
		}
	}

	public static async Task<UserEntity> FirstOrUnauthorizedAsync(this IQueryable<UserEntity> query, Expression<Func<UserEntity, bool>> predicate,
		CancellationToken ct = default)
	{
		var user = await query.FirstOrDefaultAsync(predicate, ct);
		return user ?? throw AppException.Unauthorized;
	}

	public static async Task<TSource> FirstOrUnauthorizedAsync<TSource>(this IQueryable<TSource> query, CancellationToken ct = default)
	{
		var user = await query.FirstOrDefaultAsync(ct);
		return user ?? throw AppException.Unauthorized;
	}

	public static async Task<TSource> FirstOrNotFoundAsync<TSource>(this IQueryable<TSource> query, Expression<Func<TSource, bool>> predicate,
		CancellationToken ct = default)
	{
		var result = await query.FirstOrDefaultAsync(predicate, ct);
		return result ?? throw AppException.NotFound;
	}

	public static async Task<TSource> FirstOrNotFoundAsync<TSource>(this IQueryable<TSource> query, CancellationToken ct = default)
	{
		var result = await query.FirstOrDefaultAsync(ct);
		return result ?? throw AppException.NotFound;
	}
}