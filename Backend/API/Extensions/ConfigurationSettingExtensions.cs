using Application.Settings;
using Infrastructure.Configuration;

namespace API.Extensions;

public static class ConfigurationSettingExtensions
{
    public static IServiceCollection AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoSettings>(configuration.GetSection("MongoSettings"));
        services.Configure<McpServerOptions>(configuration.GetSection("McpServer"));
        return services;
    }
}
