using System;
using System.Threading;
using System.Threading.Tasks;
using Homework5.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Homework5.Services
{
    public class TestBackgroundKafkaConsumer : BackgroundService
    {
        private readonly IBatchKafkaConsumer<string, string> _consumer;
        private readonly IMessageProcessor<string, string> _messageProcessor;
        private readonly ILogger<TestBackgroundKafkaConsumer> _logger;

        public TestBackgroundKafkaConsumer(
            IBatchKafkaConsumer<string, string> consumer,
            IMessageProcessor<string, string> messageProcessor,
            ILogger<TestBackgroundKafkaConsumer> logger)
        {
            _consumer = consumer;
            _messageProcessor = messageProcessor;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                   await Task.Run(() => _consumer.Run(_messageProcessor, stoppingToken));
                }
                catch (Exception)
                {
                    _logger.LogError($"Error in {nameof(TestBackgroundKafkaConsumer)} restarting");
                }
            }
        }
    }
}