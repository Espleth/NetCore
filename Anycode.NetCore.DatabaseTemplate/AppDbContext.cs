namespace Anycode.NetCore.DatabaseTemplate;

public class AppDbContext(DbContextOptions options) : IdentityUserContext<UserEntity, Guid>(options)
{
	public override DbSet<UserEntity> Users => Set<UserEntity>();
	public DbSet<ConfigurationEntity> Configuration => Set<ConfigurationEntity>();
	public DbSet<LanguageEntity> Languages => Set<LanguageEntity>();
	public DbSet<TextEntity> Texts => Set<TextEntity>();
	public DbSet<TranslationEntity> Translations => Set<TranslationEntity>();
	public DbSet<ActiveUsersStatsEntity> ActiveUsersStats => Set<ActiveUsersStatsEntity>();

	public DbSet<RoleEntity> Roles => Set<RoleEntity>();
	public DbSet<RolePermissionEntity> RolesPermissions => Set<RolePermissionEntity>();

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

		builder.Ignore<IdentityUserClaim<long>>();
		builder.Ignore<IdentityUserLogin<long>>();
		builder.Ignore<IdentityUserToken<long>>();
	}
}