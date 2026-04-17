namespace Anycode.NetCore.DbTools.Extensions;

public static class DbExtensions
{
	public static void AddAppDbContext<TDbContext>(this IServiceCollection services, string connectionString,
		Func<NpgsqlDbContextOptionsBuilder, NpgsqlDbContextOptionsBuilder> mapEnums)
		where TDbContext : DbContext
	{
		services.AddSingleton<DbContextOptions<TDbContext>>(_ =>
		{
			var builder = new DbContextOptionsBuilder<TDbContext>();
			builder.UseNpgsql(connectionString, options =>
			{
				options.MigrationsAssembly(typeof(TDbContext).Assembly.GetName().Name);
				mapEnums(options);
			});
			builder.EnableSensitiveDataLogging();
			builder.ConfigureWarnings(a => a.Ignore(RelationalEventId.PendingModelChangesWarning));
			return builder.Options;
		});
		services.AddDbContext<TDbContext>();
		services.AddDbContextFactory<TDbContext>();
	}

	/// <summary>
	/// When we don't want to track all attached entities recursively, only this one
	/// </summary>
	public static void UpdateSingleEntity<TEntity>(this DbSet<TEntity> dbSet, TEntity entity) where TEntity : class
	{
		dbSet.Entry(entity).State = EntityState.Modified;
	}

	/// <summary>
	/// So GC can collect entities
	/// </summary>
	public static void DetachEverything(this DbContext context)
	{
		foreach (var entity in context.ChangeTracker.Entries())
		{
			entity.State = EntityState.Detached;
		}
	}

	// For data seeding
	public static void AddRangeIfNotExists<TEntity, TKey>(this DbSet<TEntity> set, IEnumerable<TEntity> entities)
		where TEntity : class, IEntity<TKey>
		where TKey : notnull
	{
		foreach (var entity in entities)
		{
			set.AddIfNotExists<TEntity, TKey>(entity);
		}
	}

	// For data seeding
	public static void AddIfNotExists<TEntity, TKey>(this DbSet<TEntity> set, TEntity entity)
		where TEntity : class, IEntity<TKey>
		where TKey : notnull
	{
		var existing = set.Find(entity.Id);
		if (existing != null)
			set.Entry(existing).CurrentValues.SetValues(entity);
		else
			set.Add(entity);
	}
}