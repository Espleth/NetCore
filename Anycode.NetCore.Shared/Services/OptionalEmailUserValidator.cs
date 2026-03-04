namespace Anycode.NetCore.Shared.Services;

public class OptionalEmailUserValidator<TUser>(IdentityErrorDescriber? errors = null) : UserValidator<TUser>(errors)
	where TUser : class
{
	public override async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
	{
		var result = await base.ValidateAsync(manager, user);
		if (result.Succeeded || !string.IsNullOrWhiteSpace(await manager.GetEmailAsync(user)))
			return result;

		var errors = result.Errors.Where(e => e.Code != "InvalidEmail").ToList();
		return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
	}
}