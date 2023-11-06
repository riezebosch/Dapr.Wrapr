using DaprDemo.Api;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();
app.UseCloudEvents();
app.MapSubscribeHandler();

app.Map();

await app.RunAsync();

public record Data(int Value);