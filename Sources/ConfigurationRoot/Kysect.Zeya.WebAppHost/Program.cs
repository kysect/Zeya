using Microsoft.Extensions.Configuration;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var useSqliteParameter = builder.AddParameter("UseSqlite");

var apiService = builder
    .AddProject<Kysect_Zeya_WebService>("apiservice")
    .WithEnvironment("UseSqlite", useSqliteParameter);

bool userSqlite = builder.Configuration.GetSection("Parameters").GetValue<bool>("UseSqlite");
if (!userSqlite)
{
    var database = builder
        .AddPostgres("zeya-db-server")
        .AddDatabase("zeya-db");

    apiService.WithReference(database);
}

builder.AddProject<Kysect_Zeya_WebClient>("webfrontend")
    .WithReference(apiService);

builder.Build().Run();
