namespace Anycode.NetCore.Shared.Attributes;

public class EnumNameAttribute : Attribute
{
	public string Name { get; private set; }

	public EnumNameAttribute(string name)
	{
		Name = name;
	}
}