using System.Threading;
using System.Threading.Tasks;

namespace SynopsisSI.Services.OrderService.Application.Interfaces.MessageBus;
public interface IMessageBusPublisher
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
}