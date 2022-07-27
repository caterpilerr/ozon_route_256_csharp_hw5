using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.Kafka;
using Homework5.Contracts;
using Microsoft.Extensions.Logging;

namespace Homework5.Services
{
    public class SimpleStringMessageProcessor : IMessageProcessor<string, string>
    {
        private readonly IProcessedKafkaMessages<string, string> _processedMessages;
        private readonly ILogger<SimpleStringMessageProcessor> _logger;

        public SimpleStringMessageProcessor(
            IProcessedKafkaMessages<string, string> processedMessages,
            ILogger<SimpleStringMessageProcessor> logger)
        {
            _processedMessages = processedMessages;
            _logger = logger;
        }

        public Task Process(IReadOnlyCollection<ConsumeResult<string, string>> batch)
        { 
            _logger.LogInformation($"New Batch with length {batch.Count} received");
            foreach (var consumeResult in batch)
            {
                var message = consumeResult.Message;
                if (_processedMessages.Contains(consumeResult))
                { 
                    _logger.LogInformation($"Skipping message with key:{message.Key} and value: {message.Value} due to it was processed earlier");
                   continue; 
                }

                try
                {
                    _logger.LogInformation($"Message with key:{message.Key} and value: {message.Value} was processed");
                    // message processing logic
                    _processedMessages.Add(consumeResult);
                }
                catch(Exception e)
                {
                   _logger.LogError(e, $"Error while processing message with key:{message.Key} and value: {message.Value}"); 
                   throw;
                }
            }
            
            return Task.CompletedTask;
        }
    }
}