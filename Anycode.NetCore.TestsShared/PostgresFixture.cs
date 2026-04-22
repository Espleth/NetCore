namespace Anycode.NetCore.TestsShared;

/// <summary>
/// Reusable xUnit fixture that boots a real PostgreSQL container (via Testcontainers)
/// and exposes a typed <typeparamref name="TDbContext"/> for EF Core integration tests.
/// </summary>
/// <remarks>
/// Inherit and override <see cref="CreateDbContext"/> to wire your specific DbContext
/// with required <see cref="DbContextOptionsBuilder"/> configuration (provider options, enum mappings, etc.).
/// </remarks>
public abstract class PostgresFixture<TDbContext>(
	string image = "postgres:17",
	string database = "test_db",
	string username = "postgres",
	string password = "postgres")
	: IAsyncLifetime
	where TDbContext : DbContext
{
	private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder(image)
		.WithDatabase(database)
		.WithUsername(username)
		.WithPassword(password)
		.Build();

	/// <summary>
	/// Connection string to the started PostgreSQL container. Available after <see cref="InitializeAsync"/>.
	/// </summary>
	protected string ConnectionString { get; private set; } = null!;

	public virtual async Task InitializeAsync()
	{
		await _postgres.StartAsync();
		ConnectionString = _postgres.GetConnectionString();

		await using var db = CreateDbContext();
		await db.Database.MigrateAsync();
	}

	public virtual async Task DisposeAsync()
	{
		await _postgres.DisposeAsync();
	}

	/// <summary>
	/// Build a fresh <typeparamref name="TDbContext"/> bound to the container's <see cref="ConnectionString"/>.
	/// Override to configure provider options (e.g. <c>UseNpgsql(...)</c>, enum mappings, warning suppression).
	/// </summary>
	public abstract TDbContext CreateDbContext();
}