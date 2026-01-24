using CoreAPI.Extensions;
using CoreAPI.Middleware;
using Vehicle.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Swagger/OpenAPI with personId header documentation
builder.Services.AddPersistence();

// Add persistence (MasterDbContext, PersonContext, DbContextFactory, Repositories)
builder.Services.AddPersistence(builder.Configuration);

// Register repositories
builder.Services.AddRepositories();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

// Enable routing so endpoint metadata is available
app.UseRouting();

// Register PersonContextMiddleware after routing but before authorization
app.UseMiddleware<PersonContextMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
