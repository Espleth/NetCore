using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using StackExchange.Redis;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using NLogLogger = NLog.ILogger;

namespace Anycode.NetCore.Shared;

public static class StartupHelper
{
	public static void ConfigureHostLogging(this IHostApplicationBuilder builder)
	{
		SetupEnvironment();
		builder.Logging.ClearProviders();
		builder.Logging.AddNLog();
	}

	public static (WebApplication app, ILogger log) CreateWebApplication(string[] args,
		Action<IServiceCollection, IConfigurationManager, NLogLogger> registerServices)
	{
		var (log, environment) = SetupEnvironment();

		var builder = WebApplication.CreateBuilder(args);
		builder.Configuration.AddJsonFile($"Config{Path.DirectorySeparatorChar}appsettings.{environment}.json", false);
		builder.Configuration.AddEnvironmentVariables(); // Env vars always override JSON files (Aspire, Docker, etc.)

		// Explicit Kestrel endpoints from appsettings override ASPNETCORE_URLS.
		// Under orchestrators (Aspire) re-point them to the assigned URL so ports match.
		if (Environment.GetEnvironmentVariable("ASPNETCORE_URLS") is { } urls)
			builder.Configuration.AddInMemoryCollection([new("Kestrel:Endpoints:Http:Url", urls.Split(';')[0])]);

		log.Info("Application environment: {Environment}", environment);
		builder.WebHost.UseKestrel();
		builder.Logging.ClearProviders();
		builder.Logging.AddNLogWeb();

		registerServices(builder.Services, builder.Configuration, log);

		var app = builder.Build();
		return (app, app.Logger);
	}

	public static async Task LaunchAsync(this WebApplication app)
	{
		try
		{
			app.Logger.Info($"Application starting...");
			await app.RunAsync();
		}
		catch (Exception e)
		{
			app.Logger.Fatal(e, "Application crashed: {Message}", e.Message);
			throw;
		}
		finally
		{
			app.Logger.Info($"Application stopped");
		}
	}

	private static (NLogLogger log, string? environment) SetupEnvironment()
	{
		CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
		CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

		var assembly = AppDomain.CurrentDomain.FriendlyName;
		GlobalDiagnosticsContext.Set("Service", assembly);

		var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLowerInvariant();

		var nlogEnvPath = Path.Combine(AppContext.BaseDirectory, "Config", $"nlog.{environment}.config");
		var defaultNlogPath = Path.Combine(AppContext.BaseDirectory, "Config", "nlog.config");
		var selectedNlogPath = Path.Exists(nlogEnvPath) ? nlogEnvPath : defaultNlogPath;

		LogManager.Setup().LoadConfigurationFromFile(selectedNlogPath);

		var log = LogManager.GetCurrentClassLogger();
		log.Info("NLog config loaded from: {NLogConfigPath}", selectedNlogPath);

		return (log, environment);
	}

	public static IServiceCollection AddCacheService<TCacheService>(this IServiceCollection services)
		where TCacheService : class, ICacheWarmupService
	{
		services.AddTransient<TCacheService>();
		services.AddTransient<ICacheWarmupService, TCacheService>();
		return services;
	}

	public static IServiceCollection AddCacheServiceSingleton<TCacheService>(this IServiceCollection services)
		where TCacheService : class, ICacheWarmupService
	{
		services.AddSingleton<TCacheService>();
		services.AddSingleton<ICacheWarmupService, TCacheService>();
		return services;
	}

	public static TService AddConfig<TService>(this IServiceCollection services, IConfiguration configuration)
		where TService : class
	{
		var config = configuration.GetConfig<TService>();
		services.AddSingleton(config);
		return config;
	}

	public static bool TryAddConfig<TService>(this IServiceCollection services, IConfiguration configuration, out TService? config)
		where TService : class
	{
		var configFound = configuration.TryGetConfig(out config);
		if (!configFound)
			return false;

		services.AddSingleton(config!);
		return true;
	}

	public static TService GetConfig<TService>(this IConfiguration configuration) where TService : class
	{
		var config = configuration.GetSection(typeof(TService).Name).Get<TService>();
		if (config == null)
			throw new Exception($"Configuration section {typeof(TService).Name} not found");

		return config;
	}

	public static bool TryGetConfig<TService>(this IConfiguration configuration, out TService? config)
		where TService : class
	{
		config = configuration.GetSection(typeof(TService).Name).Get<TService>();
		if (config == null)
			return false;

		return true;
	}

	public static IServiceCollection AddOptions<TOptions>(this IServiceCollection services, IConfiguration configuration)
		where TOptions : class
	{
		return services.Configure<TOptions>(configuration.GetSection(typeof(TOptions).Name));
	}

	public static IApplicationBuilder UseHealthChecksExt(this IApplicationBuilder app)
	{
		return app.UseHealthChecks("/h3alz", new HealthCheckOptions // TODO[low] better way to restrict access
		{
			ResponseWriter = async (context, report) =>
			{
				context.Response.ContentType = MediaTypeNames.Application.Json;

				var response = new HealthCheckResponse
				{
					Status = report.Status,
					Checks = report.Entries.Select(entry => new HealthCheckEntry
					{
						Name = entry.Key,
						Status = entry.Value.Status,
						Exception = entry.Value.Exception?.Message,
						Description = entry.Value.Description,
						Duration = entry.Value.Duration,
					}).ToList(),
					Duration = report.TotalDuration,
				};

				await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonHelper.JsonApiOptions));
			},

			ResultStatusCodes =
			{
				[HealthStatus.Healthy] = StatusCodes.Status200OK,
				[HealthStatus.Degraded] = StatusCodes.Status200OK,
				[HealthStatus.Unhealthy] = StatusCodes.Status200OK,
			},
		});
	}

	public static IServiceCollection AddAuthenticationWithJwt(this IServiceCollection services, IConfigurationManager configuration)
	{
		services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(options =>
			{
				var jwtOptions = configuration.GetConfig<JwtConfig>();

				options.Events = new JwtBearerEvents
				{
					OnMessageReceived = context =>
					{
						context.Token = context.Request.Cookies["Authorization"];
						return Task.CompletedTask;
					}
				};

				options.TokenValidationParameters = new TokenValidationParameters
				{
					NameClaimType = "sub",

					RequireExpirationTime = true,
					RequireSignedTokens = true,

					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,

					ValidIssuer = jwtOptions.ValidIssuer,
					ValidAudience = jwtOptions.ValidAudience,
					IssuerSigningKey = JwtTokenBuilder.CreateSecurityKey(jwtOptions.SecretKey)
				};
				options.MapInboundClaims = false;
				options.SaveToken = true;
			});

		return services;
	}

	public static IServiceCollection AddRedis(this IServiceCollection services, string connectionString)
	{
		var multiplexer = ConnectionMultiplexer.Connect(connectionString);
		services.AddSingleton(multiplexer.GetDatabase());
		return services;
	}
}