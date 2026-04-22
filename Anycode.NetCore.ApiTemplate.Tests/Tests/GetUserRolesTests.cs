namespace Anycode.NetCore.ApiTemplate.Tests.Tests;

/// <summary>
/// Template integration test that demonstrates the recommended way to test an endpoint's
/// <c>HandleAsync</c> against a real PostgreSQL database (containerized via Testcontainers):
/// <list type="bullet">
///   <item>Inherit the shared <see cref="PostgresFixture"/> via <c>[Collection]</c>.</item>
///   <item>Seed the minimal entity graph required by the scenario.</item>
///   <item>Build a <see cref="UserContext"/> from a faked <see cref="IHttpContextAccessor"/>
///         carrying the <c>sub</c> claim.</item>
///   <item>Invoke the endpoint's static <c>HandleAsync</c> directly — no HTTP pipeline.</item>
///   <item>Use unique IDs per test class to avoid cross-class collisions in the shared DB.</item>
/// </list>
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public class GetUserRolesTests(PostgresFixture fixture)
{
	private static readonly Guid _userId = new("11111111-1111-1111-1111-111111111111");
	private const int RoleId = 9001;
	private const int LanguageId = 9001;

	[Fact]
	public async Task ReturnsRoleAndPermissions_ForAuthorizedUser()
	{
		await using var db = fixture.CreateDbContext();
		await SeedAsync(db);

		var userContext = CreateUserContext(_userId);

		var result = await GetUserRoles.HandleAsync(db, userContext, CancellationToken.None);

		Assert.Equal(RoleId, result.RoleId);
		Assert.Equal("Tester", result.RoleName);
		Assert.Equal([Permission.Login], result.Permissions);
	}

	private static async Task SeedAsync(AppDbContext db)
	{
		// Idempotent seed: skip if the user already exists from a previous run against the shared container.
		if (await db.Users.AnyAsync(x => x.Id == _userId))
			return;

		db.Languages.Add(new LanguageEntity { Id = LanguageId, Code = "en", Name = "English" });
		db.Roles.Add(new RoleEntity
		{
			Id = RoleId,
			Name = "Tester",
			Permissions = [new RolePermissionEntity { RoleId = RoleId, Permission = Permission.Login }],
		});
		db.Users.Add(new UserEntity
		{
			Id = _userId,
			UserName = "tester",
			NormalizedUserName = "TESTER",
			RoleId = RoleId,
			LanguageId = LanguageId,
			RegistrationDate = DateTimeOffset.UtcNow,
			LastActivity = DateTimeOffset.UtcNow,
			RegistrationIp = null,
			RegistrationCountryCode = null,
			RegistrationUserAgent = null,
		});
		await db.SaveChangesAsync();
	}

	private static UserContext CreateUserContext(Guid userId)
	{
		var httpContext = new DefaultHttpContext
		{
			User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", userId.ToString())], "test")),
		};

		var accessor = Substitute.For<IHttpContextAccessor>();
		accessor.HttpContext.Returns(httpContext);

		return new UserContext(accessor);
	}
}