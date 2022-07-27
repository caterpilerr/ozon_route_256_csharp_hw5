using Confluent.Kafka;
using Homework5.Contracts;
using StackExchange.Redis;

namespace Homework5.Services
{
    public class ProcessedKafkaMessages<TKey, TValue> : IProcessedKafkaMessages<TKey, TValue>
    {
        private readonly IConnectionMultiplexer _redis;
        private const string Set = "ProcessedKafkaMessages";
    
        public ProcessedKafkaMessages(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public bool Contains(ConsumeResult<TKey, TValue> consumeResult)
        {
            var db = _redis.GetDatabase();
            var key = CreateSetKey(consumeResult);
            return db.SetContains(Set, key);
        }

        public void Add(ConsumeResult<TKey, TValue> consumeResult)
        {
            var db = _redis.GetDatabase();
            var key = CreateSetKey(consumeResult);
            db.SetAdd(Set, key);
        }

        private static string CreateSetKey(ConsumeResult<TKey, TValue> consumeResult)
        {
            return $"{consumeResult.Topic}_{consumeResult.Partition.Value}_{consumeResult.Offset.Value}";
        }
    }
}