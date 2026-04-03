using MassTransit;
using Quartz;
using Scalar.AspNetCore;
using IStartupLogger = NLog.ILogger;

var (app, _) = StartupHelper.CreateWebApplication(args, ConfigureServices);

ConfigureEndpoints();
ConfigureApplication();

await app.LaunchAsync();
return;

void ConfigureServices(IServiceCollection services, IConfigurationManager configuration, IStartupLogger startupLog)
{
	var environmentConfig = configuration.GetConfig<EnvironmentConfig>();

	services.AddOpenApi(c =>
	{
		c.AddDocumentTransformer((document, _, _) =>
		{
			document.Info.Title = "Example Template API";
			document.Servers = []; // Without this Scalar UI tries to send http requests while opened on https
			return Task.CompletedTask;
		});
		c.AddScalarTransformers();
		c.AddDocumentTransformer<OpenApiBearerSecurityTransformer>();
		c.AddDocumentTransformer<OpenApiEnumDocumentTransformer>();
		c.AddSchemaTransformer<OpenApiDateTimeTransformer>();
		c.AddSchemaTransformer<OpenApiGuidTransformer>();
		c.AddOperationTransformer<OpenApiAddAuthorizationTransformer>();
		c.AddOperationTransformer<OpenApiShowHeadersTransformer>();
	});

	services.ConfigureHttpJsonOptions(options => options.SerializerOptions.SetApiJsonSerializerOptions());

	services.AddMemoryCache() // IMemoryCache
        .AddHttpClient() // IHttpClientFactory
		.AddOutputCache() // Response cache for MinimalAPI;
		.AddValidation() // MinimalAPI model validation
		.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true) // Throw exceptions on validation errors (to them in our middleware)
		.AddProblemDetails(); // ProblemDetails (RFC 7807) for error handling

	services.AddAuthorization()
		.AddAuthenticationWithJwt(configuration);

	services.AddCors(options =>
	{
		options.AddDefaultPolicy(builder => builder.WithOrigins(environmentConfig.CorsOrigins.ToArray())
			.AllowAnyMethod()
			.AllowAnyHeader()
			.AllowCredentials());
	});

	services.AddIdentityCore<UserEntity>(options =>
		{
			options.SignIn.RequireConfirmedAccount = false;
			options.User.RequireUniqueEmail = true;
			options.Password.RequireDigit = false;
			options.Password.RequiredLength = 6;
			options.Password.RequiredUniqueChars = 3;
			options.Password.RequireNonAlphanumeric = false;
			options.Password.RequireUppercase = false;
			options.Password.RequireLowercase = false;
			options.Lockout = new LockoutOptions
			{
				AllowedForNewUsers = true,
				DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5),
				MaxFailedAccessAttempts = 10
			};
		})
		.AddEntityFrameworkStores<AppDbContext>()
		.AddSignInManager<AppSignInManager>()
		.AddDefaultTokenProviders();

	services.AddDataProtection()
		.PersistKeysToDbContext<AppDbContext>();

	services.Configure<DataProtectionTokenProviderOptions>(options => { options.TokenLifespan = TimeSpan.FromDays(14); });

	ConfigureCustomServices(services, configuration, startupLog);
}

void ConfigureCustomServices(IServiceCollection services, IConfigurationManager configuration, IStartupLogger startupLog)
{
	// Configuration
	var connections = services.AddConfig<ConnectionStrings>(configuration);
	services.AddConfig<EnvironmentConfig>(configuration);
	services.AddConfig<LoggingConfig>(configuration);
	services.AddConfig<JwtConfig>(configuration);

	// Services, common
	services.AddTransient<UserValidatorService>();
	services.AddTransient<AuthHelperService>();
	services.AddScoped<UserContext>();
	services.AddScoped<WebContext>();
	services.AddSingleton<ILookupNormalizer, LowerInvariantLookupNormalizer>();
	services.AddTransient<BotsDetectionService>();
	services.AddTransient<LastActivitiesService>();

	// Notifications
	services.AddTransient<EmailQueueService>();

	// Cache. Order might be important for cache warmup, as some services depend on others.
	services.AddTransient<IUserStampCacheService<Guid>, UserStampCacheService>();
	services.AddTransient<UserStampCacheService>();
	services.AddCacheServiceSingleton<LanguagesCacheService>();

	services.AddHostedService<CacheWarmupHostedService>();

	// Infra
	services.AddAppDbContext(connections.AppDb!);
	services.AddRedis(connections.Redis!);

	ConfigureMassTransit(services, configuration);
	ConfigureQuartz(services, configuration, startupLog);

	services.AddHealthChecks() // RabbitMQ is checked by MassTransit automatically
		.AddNpgSql(connections.AppDb!)
		.AddRedis(connections.Redis!)
		.AddCheck<ApiHealthCheck>("API");
}

void ConfigureMassTransit(IServiceCollection services, IConfigurationManager configuration)
{
	var connections = configuration.GetConfig<ConnectionStrings>();
	services.AddMassTransit(connections.RabbitMq!, mtConfig =>
	{
		mtConfig.AddConsumer<SendEmailRequestConsumer>(consConfig => consConfig.UseMessageRetry(r =>
				r.Exponential(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(5))))
			.Endpoint(endpoint => { endpoint.Name = RabbitNames.SendEmailQueue; });
	}, rabbitConfig => { rabbitConfig.SetupPublisher<SendEmailRequest>(RabbitNames.SendEmailExchange); });
}

void ConfigureQuartz(IServiceCollection services, IConfigurationManager configuration, IStartupLogger startupLog)
{
	var config = configuration.GetConfig<JobsConfig>();
	if (!config.RunJobs)
		return;

	services.AddQuartz(q => { q.AddJobIfPresent<LastActivitiesJob>(startupLog, config.LastActivitiesCron); });

	services.AddQuartzHostedService(q =>
	{
		q.AwaitApplicationStarted = true;
		q.WaitForJobsToComplete = true;
	});
}

void ConfigureEndpoints()
{
	var endpointsBuilder = app.MapGroup("/api")
		.AddEndpointFilter<AuthFilter<Guid>>()
		.AddEndpointFilter<ProhibitInProductionFilter>()
		.WithMetadata(new ProducesResponseTypeMetadata(400, typeof(ErrorModel)));

	EndpointsConfigurator.Configure(endpointsBuilder);
}

void ConfigureApplication()
{
	if (!app.Environment.IsDevelopment())
		app.UseHsts();

	app.UseMiddleware<RequestLoggingMiddleware>();
	app.UseMiddleware<ExceptionHandlingMiddleware>();

	app.UseCors();
	app.UseStaticFiles();
	app.UseRouting();
	app.UseOutputCache(); // Response cache for MinimalAPI

	JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
	app.UseAuthentication();
	app.UseAuthorization();

	app.UseHealthChecksExt();

	var envConfig = app.Services.GetRequiredService<EnvironmentConfig>();
	if (envConfig.OpenApiRoutePrefix == null)
	{
		app.MapOpenApi();
		app.MapScalarApiReference();
	}
	else
	{
		// ReSharper disable once RouteTemplates.SyntaxError
		app.MapOpenApi($"/{envConfig.OpenApiRoutePrefix}/openapi/{{documentName}}.json");
		app.MapScalarApiReference(envConfig.OpenApiRoutePrefix,
			options => { options.OpenApiRoutePattern = $"/{envConfig.OpenApiRoutePrefix}/openapi/{{documentName}}.json"; });
	}
}