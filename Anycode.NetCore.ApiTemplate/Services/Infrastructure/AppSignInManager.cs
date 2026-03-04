namespace Anycode.NetCore.ApiTemplate.Services.Infrastructure;

public class AppSignInManager(
	UserManager<UserEntity> userManager,
	IHttpContextAccessor contextAccessor,
	IUserClaimsPrincipalFactory<UserEntity> claimsFactory,
	IOptions<IdentityOptions> optionsAccessor,
	ILogger<SignInManager<UserEntity>> logger,
	IAuthenticationSchemeProvider schemes,
	IUserConfirmation<UserEntity> confirmation)
	: SignInManager<UserEntity>(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation);