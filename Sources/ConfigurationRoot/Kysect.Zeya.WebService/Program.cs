using Kysect.Zeya.DependencyManager;
using Kysect.Zeya.ServiceDefaults;
using Kysect.Zeya.WebApi;

var builder = WebApplication.CreateBuilder(args);
// Add service defaults & Aspire components.
builder.AddServiceDefaults();
// Add services to the container.
builder.Services.AddProblemDetails();
// Add services to the container.
builder.Services.AddControllers()
    .AddApplicationPart(typeof(IWebApiMarker).Assembly);

builder.Services.AddSwaggerGen();

builder.Services
    .AddZeyaConfiguration()
    .AddZeyaRequiredService();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapDefaultEndpoints();
app.Run();
