var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.Kysect_Zeya_WebService>("apiservice");

builder.AddProject<Projects.Kysect_Zeya_WebClient>("webfrontend")
    .WithReference(apiService);

builder.Build().Run();
