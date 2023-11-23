using AspireDemo.Domain;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedisContainer(Constants.ContainerNames.RedisContainer);
var queue = builder.AddRabbitMQContainer(Constants.ContainerNames.RabbitMqContainer);

var postgres = builder
  .AddPostgresContainer(Constants.ContainerNames.PostgresContainer)
  .WithVolumeMount("postgresvolume", "/var/lib/postgresql/data", VolumeMountType.Named)
  .AddDatabase(Constants.ContainerNames.PostgresDatabase);

var apiservice = builder
  .AddProject<Projects.AspireDemo_ApiService>(Constants.ServiceNames.ApiService)
  .WithReference(postgres)
  .WithReference(queue);

builder.AddProject<Projects.AspireDemo_Web>(Constants.ServiceNames.FrontendService)
    .WithReference(cache)
    .WithReference(apiservice);

builder.AddProject<Projects.AspireDemo_IndexService>(Constants.ServiceNames.IndexService)
  .WithReference(queue);

builder.Build().Run();