using MassTransit;
using Microsoft.Extensions.Logging;
using SynopsisSI.Services.OrderService.Application.Interfaces.MessageBus;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SynopsisSI.Services.OrderService.Infrastructure.EventBus;
public class MassTransitMessageBusPublisher : IMessageBusPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<MassTransitMessageBusPublisher> _logger;

    public MassTransitMessageBusPublisher(IPublishEndpoint publishEndpoint, ILogger<MassTransitMessageBusPublisher> logger)
    {
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        _logger.LogInformation("Publishing event of type {EventType}: {@Event}", typeof(T).Name, message);
        await _publishEndpoint.Publish(message, cancellationToken);
    }
}