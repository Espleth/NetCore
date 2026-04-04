FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG DEPLOY_ENVIRONMENT=development
WORKDIR /src
COPY ["Anycode.NetCore.ApiTemplate/Anycode.NetCore.ApiTemplate.csproj", "Anycode.NetCore.ApiTemplate/"]
COPY ["Anycode.NetCore.DatabaseTemplate/Anycode.NetCore.DatabaseTemplate.csproj", "Anycode.NetCore.DatabaseTemplate/"]
COPY ["Anycode.NetCore.DbTools/Anycode.NetCore.DbTools.csproj", "Anycode.NetCore.DbTools/"]
COPY ["Anycode.NetCore.Shared/Anycode.NetCore.Shared.csproj", "Anycode.NetCore.Shared/"]
COPY ["Red.Backend.Api/Config/appsettings.json", "Red.Backend.Api/Config/"]
COPY ["Red.Backend.Api/Config/appsettings.${DEPLOY_ENVIRONMENT}.json", "Red.Backend.Api/Config/"]
COPY ["Red.Backend.Api/Config/nlog.${DEPLOY_ENVIRONMENT}.config", "Red.Backend.Api/Config/"]
RUN dotnet restore "Anycode.NetCore.ApiTemplate/Anycode.NetCore.ApiTemplate.csproj"
COPY . .
WORKDIR "/src/Anycode.NetCore.ApiTemplate"
RUN dotnet build "./Anycode.NetCore.ApiTemplate.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG DEPLOY_ENVIRONMENT=development
RUN dotnet publish "./Anycode.NetCore.ApiTemplate.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true
# Remove config files that don't belong to the target environment
RUN find /app/publish/Config -name "appsettings.*.json" ! -name "appsettings.${DEPLOY_ENVIRONMENT}.json" -delete 2>/dev/null; \
    find /app/publish/Config -name "nlog.*.config" ! -name "nlog.${DEPLOY_ENVIRONMENT}.config" ! -name "nlog.common.config" -delete 2>/dev/null; \
    true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Anycode.NetCore.ApiTemplate.dll"]
