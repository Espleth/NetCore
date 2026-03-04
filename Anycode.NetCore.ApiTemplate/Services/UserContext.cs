namespace Anycode.NetCore.ApiTemplate.Services;

public class UserContext(IHttpContextAccessor httpContextAccessor) : BaseUserContext<Guid>(httpContextAccessor);