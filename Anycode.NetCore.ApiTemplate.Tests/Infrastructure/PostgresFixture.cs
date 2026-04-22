namespace Anycode.NetCore.ApiTemplate.Tests.Infrastructure;

/// <summary>
/// Concrete fixture for <see cref="AppDbContext"/>: configures Npgsql with the
/// project's enum mappings (<see cref="DbExtensions.MapEnums"/>) and silences
/// the harmless "pending model changes" warning that EF emits when running
/// migrations against a freshly built schema.
/// </summary>
public class PostgresFixture() : PostgresFixture<AppDbContext>(database: "anycode_apitemplate_test")
{
	public override AppDbContext CreateDbContext()
	{
		var builder = new DbContextOptionsBuilder<AppDbContext>();
		builder.UseNpgsql(ConnectionString, options => options.MapEnums());
		builder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
		return new AppDbContext(builder.Options);
	}
}