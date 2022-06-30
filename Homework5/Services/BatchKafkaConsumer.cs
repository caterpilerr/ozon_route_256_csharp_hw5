using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Homework5.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Homework5.Services
{
    public class BatchKafkaConsumer<TKey, TValue> : IBatchKafkaConsumer<TKey, TValue>
    {
        private readonly ConsumerConfig _config;
        private readonly ILogger<BatchKafkaConsumer<TKey, TValue>> _logger;
        private readonly int _maxBatchSize;
        private readonly int _batchCollectingTimeLimit;
        private readonly string _topic;

        public BatchKafkaConsumer(
            ILogger<BatchKafkaConsumer<TKey, TValue>> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _maxBatchSize = int.Parse(configuration.GetSection("Kafka:BatchSize").Value);
            _batchCollectingTimeLimit = int.Parse(configuration.GetSection("Kafka:BatchingTimeLimitMs").Value);
            _topic = configuration.GetSection("Kafka:Topic").Value;
            _config = new ConsumerConfig
            {
                BootstrapServers = configuration.GetSection("Kafka:Server").Value,
                GroupId = "batchConsumer1",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoOffsetStore = false,
            };
        }

        public async Task Run(IMessageProcessor<TKey, TValue> processor, CancellationToken token)
        {
            using var consumer = new ConsumerBuilder<TKey, TValue>(_config).Build();
            consumer.Subscribe(_topic);

            var batch = new List<ConsumeResult<TKey, TValue>>();
            while (!token.IsCancellationRequested)
            {
                var tokenSource = new CancellationTokenSource();
                var timer = new Timer(state =>
                    {
                        if (state is CancellationTokenSource ts)
                        {
                            ts.Cancel();
                        }
                    },
                    tokenSource,
                    _batchCollectingTimeLimit,
                    0);

                while (batch.Count < _maxBatchSize)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(tokenSource.Token);
                        if (consumeResult.IsPartitionEOF)
                        {
                            continue;
                        }

                        batch.Add(consumeResult);
                    }
                    catch (ConsumeException e)
                    {
                        _logger.LogError(e, "Error while trying to consume message from topic");
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation($"Time limit {_batchCollectingTimeLimit} ms for batch collecting exceeded");
                        break;
                    }
                }

                if (batch.Count > 0)
                {
                    try
                    {
                        await processor.Process(batch);
                        foreach (var consumeResult in batch)
                        {
                            consumer.StoreOffset(consumeResult);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Error while trying to process messages");
                        throw;
                    }
                }

                batch.Clear();
            }

            consumer.Close();
        }
    }
}