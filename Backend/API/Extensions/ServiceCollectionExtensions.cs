using Application.Common.Events;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Publisher;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Security;
using Application.Common.Interfaces.Services;
using Application.Common.Security;
using Domain.Repositories.Base;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Base;
using Infrastructure.Security;
using Infrastructure.Services;

namespace API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddSingleton<IDatabaseContext, DatabaseContext>();

        // Distributed cache (Redis). If no connection string is configured we fall
        // back to a process-local in-memory distributed cache so the app still runs
        var redisConnection = configuration.GetConnectionString("Redis");
        if (string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddDistributedMemoryCache();
        }
        else
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "blueprint:";
            });
        }
        services.AddSingleton<ICacheService, RedisCacheService>();

        // Domain-event dispatch happens at the persistence boundary (Repository).
        services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();

        // Repositories
        services.AddSingleton<IRepository, Repository>();
        services.AddSingleton<ICourseRepository, CourseRepository>();
        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<IBlogPostRepository, BlogPostRepository>();
        services.AddSingleton<IBlogCommentRepository, BlogCommentRepository>();
        services.AddSingleton<IBlogLikeRepository, BlogLikeRepository>();

        services.AddScoped<IMessageBus, MessageBus>();

        // Auth / security services
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IAuthValidator, AuthValidator>();

        return services;
    }
}
