using API.Extensions;
using Application.Common.Interfaces.Services;
using Infrastructure.Configuration;
using Infrastructure.Llm;
using Infrastructure.MCP;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddConfigurationSettings(builder.Configuration);
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationServices();
builder.Services.AddMediatRAndMasstransit();


// mcp configuration and services
builder.Services.Configure<McpServerOptions>(builder.Configuration.GetSection("McpServer"));
builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection("GeminiOptions"));
builder.Services.Configure<ClaudeOptions>(builder.Configuration.GetSection("ClaudeOptions"));


builder.Services.AddMcpServer()
    .WithHttpTransport()
    .AddAuthorizationFilters()
    .WithToolsFromAssembly(typeof(Program).Assembly);

builder.Services.AddSingleton<IMcpService, McpService>();
// After Kestrel is listening, connect in-process MCP client to MapMcp endpoint.
builder.Services.AddHostedService<McpStartupService>();
builder.Services.AddSingleton<ILlmFactory, LlmFactory>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200", "https://frontend-v1-0-4m1l.onrender.com")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

// Register the background service
// builder.Services.AddHostedService<HeartbitTestJob>();

var app = builder.Build();
app.UseCors("AllowAngular");

if (app.Environment.IsDevelopment())
{
    // Swagger UI...
    app.UseSwagger();
    app.UseSwaggerUI();
}

// authentication & authorization middlewares...
app.UseAuthentication();
app.UseAuthorization();
app.UseGlobalExceptionMiddleware();

app.MapControllers();
app.MapMcp("/mcp");
app.Run();
