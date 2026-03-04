namespace Anycode.NetCore.DatabaseTemplate.DataSeeding;

public static class AppDbCommonDataSeeder
{
	public static void SeedData(AppDbContext db)
	{
		db.Languages.AddRangeIfNotExists<LanguageEntity, int>(new List<LanguageEntity>
		{
			new()
			{
				Id = 1,
				Code = "en",
				Name = "English",
			},
			new()
			{
				Id = 2,
				Code = "ru",
				Name = "Russian",
			},
		});

		db.Roles.AddRangeIfNotExists<RoleEntity, int>(new List<RoleEntity>
		{
			new()
			{
				Id = 1,
				Name = "Admin",
				Comment = "Test admin role",
			},
		});

		// RolePermissionEntity имеет составной ключ (RoleId, Permission), поэтому AddRangeIfNotExists не подходит
		var rolePermissions = new List<RolePermissionEntity>
		{
			new()
			{
				RoleId = 1,
				Permission = Permission.Login,
				CanWrite = true,
				Comment = "Test permission",
			},
		};

		foreach (var rolePermission in rolePermissions)
		{
			var exists = db.RolesPermissions.Any(rp =>
				rp.RoleId == rolePermission.RoleId &&
				rp.Permission == rolePermission.Permission);

			if (!exists)
				db.RolesPermissions.Add(rolePermission);
			else
				db.RolesPermissions.Update(rolePermission);
		}
	}
}