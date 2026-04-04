namespace Anycode.NetCore.Shared.Services.Cache;

/// <summary>
/// Basic entities in-memory cache service to keep all entities from selected table in memory.
/// Designed to be singleton
/// </summary>
public abstract class EntityCacheService<TKey, TEntity, TContext>(IDbContextFactory<TContext> dbFactory)
	: ICacheWarmupService
	where TEntity : class, IEntity<TKey>
	where TKey : notnull
	where TContext : DbContext
{
	// ReSharper disable once StaticMemberInGenericType
	protected static FrozenDictionary<TKey, TEntity> Cache = FrozenDictionary<TKey, TEntity>.Empty;

	// ReSharper disable once StaticMemberInGenericType
	private static DateTimeOffset _lastUpdate = DateTimeOffset.MinValue;

	public virtual TimeSpan? UpdateInterval => null;

	public DateTimeOffset LastUpdate => _lastUpdate;
	protected abstract IQueryable<TEntity> GetEntities(TContext db);

	public IReadOnlyDictionary<TKey, TEntity> GetAllEntities()
	{
		return Cache;
	}

	public virtual async Task UpdateAllEntitiesAsync(CancellationToken ct)
	{
		await using var db = await dbFactory.CreateDbContextAsync(ct);
		var entities = await GetEntities(db).AsNoTracking().ToListAsync(ct);
		Cache = entities.ToFrozenDictionary(x => x.Id);
		_lastUpdate = DateTimeOffset.UtcNow;
	}

	public Task WarmupAsync(CancellationToken ct)
	{
		return UpdateAllEntitiesAsync(ct);
	}
}