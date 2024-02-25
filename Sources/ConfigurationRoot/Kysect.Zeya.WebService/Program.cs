using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.DependencyManager;
using Kysect.Zeya.ServiceDefaults;
using Kysect.Zeya.WebApi;

var builder = WebApplication.CreateBuilder(args);

// Aspire configuration
builder
    .AddServiceDefaults()
    .AddNpgsqlDbContext<ZeyaDbContext>("zeya-db");

// Add services to the container.
builder.Services
    .AddEndpointsApiExplorer()
    .AddProblemDetails()
    .AddSwaggerGen();

// Add services to the container.
builder.Services.AddControllers()
    .AddApplicationPart(typeof(IWebApiMarker).Assembly);

builder.Services
    .AddZeyaConfiguration()
    .AddZeyaLocalHandlingService();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapDefaultEndpoints();

using (IServiceScope serviceScope = app.Services.CreateScope())
{
    ServiceInitialize.InitializeDatabase(serviceScope.ServiceProvider);
}

app.Run();
