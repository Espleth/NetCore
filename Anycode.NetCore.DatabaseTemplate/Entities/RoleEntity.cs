namespace Anycode.NetCore.DatabaseTemplate.Entities;

public class RoleEntity : IEntity<int>
{
	[Key]
	public int Id { get; init; }

	public required string Name { get; set; }

	public string Comment { get; set; } = "";

	public List<RolePermissionEntity> Permissions { get; init; } = [];
	public List<UserEntity> UsersAssigned { get; init; } = [];
}