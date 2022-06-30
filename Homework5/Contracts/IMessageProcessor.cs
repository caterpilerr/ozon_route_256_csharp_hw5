using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Homework5.Contracts
{
    public interface IMessageProcessor<TKey, TValue>
    {
        public Task Process(IReadOnlyCollection<ConsumeResult<TKey, TValue>> batch);
    }
}