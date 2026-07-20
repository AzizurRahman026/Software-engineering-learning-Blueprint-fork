using Application.Common.Events;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Publisher;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Security;
using Application.Common.Interfaces.Services;
using Application.Common.Security;
using Domain.Repositories.Base;
using Infrastructure.Grpc;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Indexing;
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

        // Ensure hot-path Mongo indexes at startup. Configurations are assembly-scanned
        // (like AddValidatorsFromAssembly): a new IMongoIndexConfiguration class is
        // picked up automatically — no changes here or in MongoIndexInitializer.
        var indexConfigurationTypes = typeof(IMongoIndexConfiguration).Assembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false }
                        && typeof(IMongoIndexConfiguration).IsAssignableFrom(t));
        foreach (var type in indexConfigurationTypes)
        {
            services.AddSingleton(typeof(IMongoIndexConfiguration), type);
        }
        services.AddHostedService<MongoIndexInitializer>();

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
        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<IPostRepository, PostRepository>();
        services.AddSingleton<IPostCommentRepository, PostCommentRepository>();
        services.AddSingleton<IPostLikeRepository, PostLikeRepository>();
        services.AddSingleton<ISubscriberRepository, SubscriberRepository>();

        services.AddScoped<IMessageBus, MessageBus>();

        // Auth / security services
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IAuthValidator, AuthValidator>();

        // Outbound gRPC client to the AiWorker service (singleton: reuses one channel).
        services.AddSingleton<IAiWorkerClient, AiWorkerGrpcClient>();

        return services;
    }
}
