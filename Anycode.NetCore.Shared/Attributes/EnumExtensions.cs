namespace Anycode.NetCore.Shared.Attributes;

public static class EnumExtensions
{
	public static T? GetAttribute<T>(this Enum enumValue) where T : Attribute
	{
		return enumValue.GetType()
			.GetMember(enumValue.ToString())
			.First()
			.GetCustomAttribute<T>();
	}

	public static string GetName(this Enum enumValue)
	{
		return enumValue.GetAttribute<EnumNameAttribute>()?.Name ?? enumValue.ToString();
	}
}