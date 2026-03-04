namespace Anycode.NetCore.ApiTemplate.Infrastructure.EndpointsMetadata;

public record RequireAdminAccessMetadata(params EnvironmentType[] RequireOnEnvironments);