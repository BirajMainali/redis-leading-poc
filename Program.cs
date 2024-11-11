using System.Text.Json;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// getting redis connection string
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");


// establishing connection with redis
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString!));

// adding cache service
builder.Services.AddSingleton<CacheService>();


// configuring cache
builder.Services.Configure<CacheConfiguration>(x =>
{
    x.Organization = builder.Configuration["Cache:Organization"]!;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var items = new List<string>() {
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/cache/{key}", (string org, string key, CacheService cacheService) =>
{
    var value = cacheService.GetOrCreateAsync<List<BlogDto>>(x =>
    {
        x.Key = "blogs";
        x.Duration = TimeSpan.FromMinutes(1);
        x.Reader = async () => (List<BlogDto>)await Task.FromResult(Enumerable.Empty<BlogDto>());
    });
    return value;
}).WithOpenApi();

app.MapGet("/flush-cache/{org}", (string org, CacheService cacheService) =>
{
    cacheService.FlushCache(org);
    return Results.Ok($"Cache flushed for tenant {org}");
}).WithOpenApi();

app.Run();


public class BlogDto
{

}
