using CoreAPI.Extensions;
using CoreAPI.Middleware;
using Vehicle.Application.Extensions;
using Vehicle.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// TODO: configure logging standard, can be done with Serilog, OpenTelemetry, etc. 
// Export to other logging databases like Loki

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddPersistence();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSecurity(builder.Configuration, builder.Environment);

var app = builder.Build();

// Initialize databases (migrate Master + all Shards)
await app.InitializeDatabasesAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CoreAPI v1");
    });
}

// Security middleware (order matters!)
if (!app.Environment.IsDevelopment())
{
    app.UseHsts(); // HSTS should only be used in production
}
app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseRouting();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<PersonContextMiddleware>();
app.UseMiddleware<AuthenticationMiddleware>();
app.UseAuthorization();
app.MapControllers().RequireRateLimiting("ApiPolicy");

app.Run();
