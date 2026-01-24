using CoreAPI.Extensions;
using CoreAPI.Middleware;
using Vehicle.Application.Extensions;
using Vehicle.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddPersistence();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CoreAPI v1");
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<AuthenticationMiddleware>();
app.UseMiddleware<PersonContextMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();
