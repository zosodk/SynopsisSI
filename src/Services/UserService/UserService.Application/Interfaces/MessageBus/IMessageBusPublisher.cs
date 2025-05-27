using System.Threading;
using System.Threading.Tasks;

namespace SynopsisSI.Services.UserService.Application.Interfaces.MessageBus;
public interface IMessageBusPublisher
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
}