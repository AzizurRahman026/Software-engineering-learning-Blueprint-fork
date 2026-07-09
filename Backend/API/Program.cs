using System.Text;
using API.Extensions;
using API.MiddleWare;
using Application.Common.Interfaces.Security;
using Application.Common.Interfaces.Services;
using Application.Settings;
using Infrastructure.Chat;
using Infrastructure.Configuration;
using Infrastructure.Llm;
using Infrastructure.MCP;
using Infrastructure.Security;
using Infrastructure.Services;
using Infrastructure.SignalR.Hubs;
using Infrastructure.SignalR.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Console logging: print scope properties (CorrelationId, Username, ...) on every log line.
builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = true;
});

builder.Services.AddConfigurationSettings(builder.Configuration);
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddMediatRAndMasstransit();


// mcp configuration and services
builder.Services.Configure<McpServerOptions>(builder.Configuration.GetSection("McpServer"));
builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection("GeminiOptions"));
builder.Services.Configure<ClaudeOptions>(builder.Configuration.GetSection("ClaudeOptions"));

// Password recovery: Brevo email transport + reset-token settings.
builder.Services.Configure<BrevoEmailOptions>(builder.Configuration.GetSection("BrevoEmail"));
builder.Services.Configure<PasswordResetOptions>(builder.Configuration.GetSection("Auth:PasswordReset"));
// Expose PasswordResetOptions as a plain value so the Application-layer handler needn't depend on IOptions.
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<PasswordResetOptions>>().Value);

// Super-admin bootstrap: the email here is elevated to SuperAdmin on signup/login.
builder.Services.Configure<SuperAdminOptions>(builder.Configuration.GetSection("Auth:SuperAdmin"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<SuperAdminOptions>>().Value);

// JWT: options + token generator. Key must be supplied (Jwt__Key env var in production).
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<JwtOptions>>().Value);
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddHttpClient<IEmailSender, BrevoEmailSender>();

builder.Services.AddSingleton<IChatHistoryStore, MongoChatHistoryStore>();
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .AddAuthorizationFilters()
    .WithToolsFromAssembly(typeof(Application.Tools.TutorialTools).Assembly);

// Singleton MCP client; connects lazily on first use and is disposed (IAsyncDisposable) on host shutdown.
builder.Services.AddSingleton<IMcpService, McpService>();
builder.Services.AddSingleton<ILlmFactory, LlmFactory>();

builder.Services.AddSignalR();
builder.Services.AddSingleton<INotificationService, SignalRNotificationService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200", "https://frontend-v1-0-4m1l.onrender.com")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

builder.Services.AddControllers();

// JWT bearer authentication. The signing key is required — fail fast if it's missing.
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwtOptions.Key))
    throw new InvalidOperationException("Jwt:Key is not configured. Set the Jwt__Key environment variable.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

// Register the background service
// builder.Services.AddHostedService<HeartbitTestJob>();

var app = builder.Build();

// Outermost middleware: open a CorrelationId log scope FIRST so every later log line
// (including the exception middleware's) carries the same id, and the id is set on
app.UseCorrelationId();

app.UseCors("AllowAngular");

if (app.Environment.IsDevelopment())
{
    // Swagger UI...
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// authentication & authorization middlewares...
app.UseAuthentication();
app.UseAuthorization();
app.UseGlobalExceptionMiddleware();

app.MapControllers();
app.MapMcp("/mcp");
app.MapHub<NotificationHub>("/notifications");
app.Run();

// Exposed so WebApplicationFactory<Program> can discover the app's entry point
// for integration tests. Top-level-statement programs are otherwise internal.
public partial class Program { }
