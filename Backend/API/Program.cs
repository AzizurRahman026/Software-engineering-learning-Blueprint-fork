using API.Extensions;
using Application.Common.Interfaces.Services;
using Infrastructure.Llm;
using Infrastructure.MCP;
using Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddConfigurationSettings(builder.Configuration);
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationServices();
builder.Services.AddMediatRAndMasstransit();

builder.Services.AddSingleton<IMcpService, McpService>();
// IHostedService: connects to MCP server before any HTTP request arrives
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
app.Run();
