namespace Anycode.NetCore.Shared.Services;

public class OptionalEmailUserValidator<TUser>(IdentityErrorDescriber? errors = null) : UserValidator<TUser>(errors)
	where TUser : class
{
	public override async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
	{
		var result = await base.ValidateAsync(manager, user);
		if (result.Succeeded || await manager.GetEmailAsync(user) != null)
			return result;

		// Optional email means null only; an empty string must remain an invalid email.
		var errors = result.Errors.Where(e => e.Code != nameof(IdentityErrorDescriber.InvalidEmail)).ToList();
		return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
	}
}