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
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Database
        services.AddSingleton<IDatabaseContext, DatabaseContext>();

        // Repositories
        services.AddSingleton<IRepository, Repository>();
        services.AddSingleton<ICourseRepository, CourseRepository>();
        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<IBlogPostRepository, BlogPostRepository>();
        services.AddSingleton<IBlogCommentRepository, BlogCommentRepository>();
        services.AddSingleton<IBlogLikeRepository, BlogLikeRepository>();

        // Message bus
        services.AddSingleton<IMessageBus, MessageBus>();

        // Auth / security services
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IAuthValidator, AuthValidator>();

        return services;
    }
}
