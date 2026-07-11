using Infrastructure.Configuration;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

/// <summary>
/// Wires MassTransit: registers every <see cref="IConsumer{T}"/> in this assembly and connects the bus
/// to RabbitMQ when a broker is configured, otherwise the in-memory transport (so local runs and hosts
/// without a broker still work). This is what actually backs <c>IMessageBus.PublishAsync</c>.
/// </summary>
public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbit = configuration.GetSection("RabbitMQ").Get<RabbitMqOptions>() ?? new RabbitMqOptions();

        services.AddMassTransit(bus =>
        {
            // Consumers live in the Infrastructure assembly (Messaging/Consumers). Assembly-scanned so a
            // new IConsumer<T> is picked up automatically — no registration here.
            bus.AddConsumers(typeof(MessagingServiceCollectionExtensions).Assembly);

            if (string.IsNullOrWhiteSpace(rabbit.Host))
            {
                // No broker configured — in-memory transport keeps the app booting (like the Redis fallback).
                bus.UsingInMemory((context, cfg) =>
                {
                    cfg.UseMessageRetry(r => r.Immediate(3));
                    cfg.ConfigureEndpoints(context);
                });
            }
            else
            {
                bus.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbit.Host, rabbit.Port, rabbit.VirtualHost, h =>
                    {
                        h.Username(rabbit.Username);
                        h.Password(rabbit.Password);
                    });

                    // Bounded retry with backoff; after the last attempt MassTransit dead-letters the
                    // message to the "<queue>_error" queue automatically.
                    cfg.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5)));
                    cfg.ConfigureEndpoints(context);
                });
            }
        });

        return services;
    }
}
