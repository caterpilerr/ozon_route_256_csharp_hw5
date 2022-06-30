using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Confluent.Kafka;
using Homework5.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Homework5.Controllers
{
    [Route("[controller]")]
    public class TestMessageController : ControllerBase
    {
        private readonly ProducerConfig _producerConfig;
        private readonly string _topic; 

        public TestMessageController(IConfiguration configuration)
        {
            _topic = configuration.GetSection("Kafka:Topic").Value;
            _producerConfig = new ProducerConfig
            {
                BootstrapServers = configuration.GetSection("Kafka:Server").Value,
                ClientId = Dns.GetHostName(),
            };
        }

        [HttpPost]
        public async Task<ActionResult>  AddMessages([FromBody]IEnumerable<TestMessage> messages)
        {
            using var producer = new ProducerBuilder<string, string>(_producerConfig).Build();
            {
                foreach (var message in messages)
                {
                    await producer.ProduceAsync(_topic, new Message<string, string> { Key = message.Key, Value = message.Value });
                }
            }

            return Ok();
        }
    }
}