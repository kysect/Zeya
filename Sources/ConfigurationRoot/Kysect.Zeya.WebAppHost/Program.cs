using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddPostgresContainer("zeya-db-server")
    .AddDatabase("zeya-db");

var apiService = builder.AddProject<Kysect_Zeya_WebService>("apiservice")
    .WithReference(database);

builder.AddProject<Kysect_Zeya_WebClient>("webfrontend")
    .WithReference(apiService);

builder.Build().Run();
