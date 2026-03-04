using Anycode.NetCore.ApiTemplate.Endpoints.App;
using Anycode.NetCore.ApiTemplate.Endpoints.Auth;
using Anycode.NetCore.ApiTemplate.Endpoints.Test;
using Anycode.NetCore.ApiTemplate.Endpoints.User;

namespace Anycode.NetCore.ApiTemplate.Endpoints;

public static class EndpointsConfigurator
{
	public static void Configure(RouteGroupBuilder endpointsBuilder)
	{
		endpointsBuilder.MapGroupWithTag("Auth")
			.Register<RegisterUser>()
			.Register<LogInUser>()
			.Register<ChangePassword>()
			.Register<RequestResetPassword>()
			.Register<ResetPassword>();
        
		endpointsBuilder.MapGroupWithTag("App")
			.Register<GetAllRoles>()
			.Register<GetAllPermissions>();

		endpointsBuilder.MapGroupWithTag("User")
			.Register<GetUserRoles>();

		endpointsBuilder.MapGroupWithTag("Test")
			.Register<TestOpenApiEndpoint>()
			.Register<TestOpenApiEndpoint2>();
	}
}