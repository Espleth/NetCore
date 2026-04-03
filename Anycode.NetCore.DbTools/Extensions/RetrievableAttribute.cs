namespace Anycode.NetCore.DbTools.Extensions;

public abstract class RetrievableAttribute<TAttribute> : Attribute where TAttribute : RetrievableAttribute<TAttribute>
{
	private static class Cache<TEnum> where TEnum : struct, Enum
	{
		public static readonly FrozenDictionary<TEnum, TAttribute?> Instance = BuildCache();

		private static FrozenDictionary<TEnum, TAttribute?> BuildCache()
		{
			var dict = new Dictionary<TEnum, TAttribute?>();
			foreach (var status in Enum.GetValues<TEnum>())
			{
				var field = typeof(TEnum).GetField(status.ToString());
				var attribute = field?.GetCustomAttribute<TAttribute>();
				dict[status] = attribute;
			}

			return dict.ToFrozenDictionary();
		}
	}

	public static TAttribute Get<TEnum>(TEnum status) where TEnum : struct, Enum
		=> Cache<TEnum>.Instance.GetValueOrDefault(status)
		   ?? throw new InvalidOperationException($"{status} has no {typeof(TAttribute).Name} attribute.");

	public static FrozenSet<TEnum> GetValues<TEnum>(Func<TAttribute, bool> predicate) where TEnum : struct, Enum
		=> Cache<TEnum>.Instance
			.Where(x => x.Value is not null && predicate(x.Value))
			.Select(x => x.Key)
			.ToFrozenSet();
}