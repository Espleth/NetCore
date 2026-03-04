namespace Anycode.NetCore.Shared.Infrastructure;

public interface IEndpoint
{
	void Register(IEndpointRouteBuilder endpointsBuilder);
}