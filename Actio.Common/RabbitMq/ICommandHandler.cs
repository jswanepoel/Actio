using System.Threading.Tasks;

namespace Actio.Common.RabbitMq
{
    public interface ICommandHandler<TCommand>
    {
        Task HandleAsync(TCommand command);
    }
}