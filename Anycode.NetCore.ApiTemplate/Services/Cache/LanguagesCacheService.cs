namespace Anycode.NetCore.ApiTemplate.Services.Cache;

public class LanguagesCacheService(IDbContextFactory<AppDbContext> dbFactory) : EntityCacheService<int, LanguageEntity, AppDbContext>(dbFactory)
{
	protected override IQueryable<LanguageEntity> GetEntities(AppDbContext db) => db.Languages;

	private static FrozenDictionary<string, LanguageEntity> _cacheByCodes = FrozenDictionary<string, LanguageEntity>.Empty;

	public override async Task UpdateAllEntitiesAsync(CancellationToken ct)
	{
		await base.UpdateAllEntitiesAsync(ct);
		_cacheByCodes = Cache.Values.ToFrozenDictionary(x => x.Code);
	}

	public IReadOnlyDictionary<string, LanguageEntity> GetAllEntitiesByCodes()
	{
		return _cacheByCodes;
	}
}