var builder = DistributedApplication.CreateBuilder(args);

var postgresPassword = builder.AddParameter("postgres-password", "root");
var rabbitmqPassword = builder.AddParameter("rabbitmq-password", "guest");

// Infrastructure
var postgres = builder.AddPostgres("postgres", password: postgresPassword)
	.WithDataVolume()
	.WithPgAdmin();

var appDb = postgres.AddDatabase("appdb", "template_app_db");

var redis = builder.AddRedis("redis")
	.WithDataVolume();

var rabbitmq = builder.AddRabbitMQ("rabbitmq", password: rabbitmqPassword)
	.WithDataVolume()
	.WithManagementPlugin();

var seq = builder.AddSeq("seq")
	.WithDataVolume()
	.WithEnvironment("ACCEPT_EULA", "Y");

// Migrations
var migrations = builder.AddProject<Projects.Anycode_NetCore_MigrationServiceTemplate>("migrations")
	.WithReference(seq).WaitFor(seq)
	.WithReference(appDb, "AppDb").WaitFor(appDb);

// API
builder.AddProject<Projects.Anycode_NetCore_ApiTemplate>("api")
	.WithExternalHttpEndpoints()
	.WithUrl("/scalar", "Scalar")
	.WithReference(seq).WaitFor(seq)
	.WithReference(appDb, "AppDb").WaitFor(appDb)
	.WithReference(redis, "Redis").WaitFor(redis)
	.WithReference(rabbitmq, "RabbitMq").WaitFor(rabbitmq)
	.WaitForCompletion(migrations);

builder.Build().Run();