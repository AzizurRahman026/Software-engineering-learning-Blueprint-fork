using Application.Common.Interfaces.Publisher;
using MassTransit;
using MediatR;

namespace Infrastructure.Services;

public class MessageBus : IMessageBus
{
    private readonly IMediator _mediator;
    private readonly IPublishEndpoint _publishEndpoint;

    public MessageBus(IMediator mediator, IPublishEndpoint publishEndpoint)
    {
        _mediator = mediator;
        _publishEndpoint = publishEndpoint;
    }

    public Task SendAsync<TCommand>(TCommand command) where TCommand : IRequest
        => _mediator.Send(command);

    public Task<TResponse> SendAsync<TCommand, TResponse>(TCommand command)
        where TCommand : IRequest<TResponse>
        where TResponse : class
        => _mediator.Send(command);

    // Publishes an event/message onto the bus (RabbitMQ, or the in-memory transport as a fallback).
    // Consumers pick it up and process it off the request thread.
    public Task PublishAsync<T>(T command) where T : class
        => _publishEndpoint.Publish(command);
}
