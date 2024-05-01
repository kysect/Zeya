using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.DependencyManager;
using Kysect.Zeya.ServiceDefaults;
using Kysect.Zeya.WebApi;
using Kysect.Zeya.WebService;

var builder = WebApplication.CreateBuilder(args);

// Aspire configuration
builder.AddServiceDefaults();

bool userSqlite = builder.Configuration.GetValue<bool>("UseSqlite");
if (userSqlite)
{
    builder.Services.AddZeyaSqliteDbContext("Database.sqlite");
}
else
{
    builder.AddNpgsqlDbContext<ZeyaDbContext>("zeya-db");
}

// Add services to the container.
builder.Services
    .AddEndpointsApiExplorer()
    .AddProblemDetails()
    .AddSwaggerGen();

// Add services to the container.
builder.Services.AddControllers()
    .AddApplicationPart(typeof(IWebApiMarker).Assembly);

builder.Services
    .AddZeyaLocalHandlingService();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

// Disable CORS
app.UseCors(corsPolicyBuilder => corsPolicyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseAuthorization();
app.MapControllers();
app.MapDefaultEndpoints();

using (IServiceScope serviceScope = app.Services.CreateScope())
{
    var webServiceStartupConfigurator = new WebServiceStartupConfigurator(serviceScope);
    await webServiceStartupConfigurator.InitializeDatabase(userSqlite);
}

await app.RunAsync();
