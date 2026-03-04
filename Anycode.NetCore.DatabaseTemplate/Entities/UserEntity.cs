namespace Anycode.NetCore.DatabaseTemplate.Entities;

public class UserEntity : IdentityUser<Guid>, ISecurityUser<Guid>
{
	public bool IsBlocked { get; set; }

	public required DateTimeOffset RegistrationDate { get; init; }
	public required DateTimeOffset LastActivity { get; set; }

	[MaxLength(DbConstraints.MaxIpLength)]
	public required string? RegistrationIp { get; init; }

	[MaxLength(DbConstraints.MaxCountryCodeLength)]
	public required string? RegistrationCountryCode { get; init; }

	[MaxLength(DbConstraints.MaxUserAgentLength)]
	public required string? RegistrationUserAgent { get; init; }

	public int LanguageId { get; set; }
	public LanguageEntity? Language { get; set; }

	public int RoleId { get; init; }
	public RoleEntity? Role { get; set; }
}

internal class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
	public void Configure(EntityTypeBuilder<UserEntity> builder)
	{
		builder.ToTable(nameof(AppDbContext.Users));

		builder.Property(x => x.Email).HasMaxLength(DbConstraints.MaxEmailLength);
		builder.Property(x => x.NormalizedEmail).HasMaxLength(DbConstraints.MaxEmailLength);
		builder.Property(x => x.UserName).HasMaxLength(DbConstraints.MaxUsernameLength);
		builder.Property(x => x.NormalizedUserName).HasMaxLength(DbConstraints.MaxUsernameLength);

		builder.Ignore(x => x.PhoneNumber);
		builder.Ignore(x => x.PhoneNumberConfirmed);
		builder.Ignore(x => x.TwoFactorEnabled);
		builder.Ignore(x => x.LockoutEnd);
		builder.Ignore(x => x.LockoutEnabled);

		builder.HasOne(x => x.Language)
			.WithMany(x => x.Users)
			.HasForeignKey(x => x.LanguageId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(x => x.Role)
			.WithMany(x => x.UsersAssigned)
			.HasForeignKey(x => x.RoleId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}