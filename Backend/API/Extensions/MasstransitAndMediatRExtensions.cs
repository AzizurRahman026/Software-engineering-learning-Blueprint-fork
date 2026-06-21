using Application.Common.Behaviors;
using Application.Features.Courses.Query.GetAllCourses;
using FluentValidation;
using MassTransit;
using MediatR;

namespace API.Extensions;

public static class MasstransitAndMediatRExtensions
{
    public static IServiceCollection AddMediatRAndMasstransit(this IServiceCollection services)
    {
        var applicationAssembly = typeof(GetAllCoursesQueryHandler).Assembly;
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(applicationAssembly);
        });

        // Discover and register every AbstractValidator in the Application assembly.
        services.AddValidatorsFromAssembly(applicationAssembly);

        // Run validation in the MediatR pipeline before any handler executes.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }
}
