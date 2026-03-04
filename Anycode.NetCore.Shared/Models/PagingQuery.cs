namespace Anycode.NetCore.Shared.Models;

[PublicAPI]
public record PagingQuery
{
	// Due to dotnet validation bug, this produces circular reference exception
	// [ValidateNever]
	// public static PagingQuery All => new(-1);

	[FromQuery(Name = "page.take")]
	[DefaultValue(10)]
	[Range(0, DefaultValidation.MaxTake)]
	public virtual int Take { get; init; } = 10;

	[FromQuery(Name = "page.skip")]
	[DefaultValue(0)]
	public int Skip { get; init; }

	public PagingQuery()
	{
	}

	public PagingQuery(int take = 10, int skip = 0)
	{
		Take = take;
		Skip = skip;
	}

	public (int from, int to) AsFromTo()
	{
		return (Skip, Skip + Take);
	}
}
