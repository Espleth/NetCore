namespace Anycode.NetCore.DatabaseTemplate.Entities;

[PrimaryKey(nameof(RoleId), nameof(Permission))]
public class RolePermissionEntity
{
	public int RoleId { get; init; }
	public RoleEntity? Role { get; init; }

	public required Permission Permission { get; init; }

	public string Comment { get; set; } = "";

	public bool CanWrite { get; init; }
}

internal class RolePermissionEntityConfiguration : IEntityTypeConfiguration<RolePermissionEntity>
{
	public void Configure(EntityTypeBuilder<RolePermissionEntity> builder)
	{
		builder.HasOne(x => x.Role)
			.WithMany(x => x.Permissions)
			.HasForeignKey(x => x.RoleId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}