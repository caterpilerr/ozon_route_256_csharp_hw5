using System.Threading;
using System.Threading.Tasks;

namespace Homework5.Contracts
{
    public interface IBatchKafkaConsumer<TKey, TValue>
    {
        public Task Run(IMessageProcessor<TKey, TValue> processor, CancellationToken cancellationToken);
    }
}