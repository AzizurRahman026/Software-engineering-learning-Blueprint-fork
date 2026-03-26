using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Publisher;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Domain.Repositories.Base;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Base;
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

        // register the message bus
        services.AddSingleton<IMessageBus, MessageBus>();


        return services;
    }
}
