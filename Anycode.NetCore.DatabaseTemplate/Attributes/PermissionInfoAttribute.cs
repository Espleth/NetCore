namespace Anycode.NetCore.DatabaseTemplate.Attributes;

public class PermissionInfoAttribute(bool isGeneral) : RetrievableAttribute<PermissionInfoAttribute>
{
	public bool IsGeneral => isGeneral;
}

internal static class PermissionInfoAttributeExtensions
{
	extension(Permission permission)
	{
		public bool IsGeneral() => PermissionInfoAttribute.Get(permission).IsGeneral;
	}
}