namespace Anycode.NetCore.Shared.Models;

[PublicAPI]
public record QueryableList<T>
{
	public static QueryableList<T> Empty(int total = 0) => new() { Data = [], Total = total };

	public required List<T> Data { get; init; }
	public required int Total { get; init; }
}