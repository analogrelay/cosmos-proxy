using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Azure.Cosmos.Proxy.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddControllers()
    .AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ProxyOptions>(builder.Configuration.GetSection("Proxy"));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<ContainerService>();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGrpcReflectionService();
}

app.Run();
