namespace Anycode.NetCore.ApiTemplate.Tests.Infrastructure;

/// <summary>
/// xUnit collection definition — ensures all integration test classes that opt in via
/// <c>[Collection(IntegrationTestCollection.Name)]</c> share a single
/// <see cref="PostgresFixture"/> instance (and therefore one PostgreSQL container).
/// </summary>
[CollectionDefinition(Name)]
public class IntegrationTestCollection : ICollectionFixture<PostgresFixture>
{
	public const string Name = "Integration";
}