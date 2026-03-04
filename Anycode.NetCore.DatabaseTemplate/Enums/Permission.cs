namespace Anycode.NetCore.DatabaseTemplate.Enums;

public enum Permission
{
	// Example permission, in real app probably all users will be able to log in.
	[PermissionInfo(true)]
	Login,
}