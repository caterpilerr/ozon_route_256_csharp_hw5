using Confluent.Kafka;

namespace Homework5.Contracts
{
    public interface IProcessedKafkaMessages<TKey, TValue>
    {
        public bool Contains(ConsumeResult<TKey, TValue> consumeResult);

        public void Add(ConsumeResult<TKey, TValue> consumeResult);
    }
}