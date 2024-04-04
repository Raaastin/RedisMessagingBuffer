using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Messaging.Buffer.Buffer;
using Messaging.Buffer.Service;
using Messaging.Buffer.Test.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Moq;

namespace Messaging.Buffer.Test.BufferTest
{
    public class SendRequestAsync_Should
    {
        private Mock<IServiceProvider> _serviceProvider;
        private Mock<IMessaging> _messaging;
        private Mock<ILogger<TestRequestBuffer>> _logger;
        private Mock<IOptions<RedisOptions>> _options;
        private TestRequestBuffer buffer;

        public SendRequestAsync_Should()
        {
            _serviceProvider = new();
            _options = new();
            _messaging = new();
            _logger = new();

            _serviceProvider.Setup(x => x.GetService(typeof(IMessaging))).Returns(_messaging.Object);
            _serviceProvider.Setup(x => x.GetService(typeof(ILogger<RequestBufferBase<TestRequest, TestResponse>>))).Returns(_logger.Object);
            _serviceProvider.Setup(x => x.GetService(typeof(TestRequest))).Returns(new TestRequest());
            _serviceProvider.Setup(x => x.GetService(typeof(IOptions<RedisOptions>))).Returns(_options.Object);            
            _options.Setup(x => x.Value).Returns(new RedisOptions()
            {
                RedisConnexionStrings = new List<string>() { "http:toast.net:999" },
                Timeout = 300
            });
        }

        /// <summary>
        /// Send a TestRequest and verify the content of TestResponse.
        /// The content of TestResponse is computed in Aggregate method.
        /// </summary>
        [Fact]
        public async void ProduceAggregatedResponse()
        {
            // Arrange
            buffer = new TestRequestBuffer(_messaging.Object, new TestRequest(), _logger.Object);
            _messaging.Setup(x => x.SubscribeResponseAsync(buffer.CorrelationId, buffer.OnResponse)).Verifiable();
            _messaging.Setup(x => x.PublishRequestAsync(buffer.CorrelationId, buffer.Request)).ReturnsAsync(2).Callback(async () =>
            {
                await Task.Delay(50);
                buffer.OnResponse(null, new ReceivedEventArgs()
                {
                    Channel = $"Response:{buffer.CorrelationId}",
                    CorrelationId = buffer.CorrelationId,
                    MessageType = $"{typeof(TestResponse)}",
                    Value = new TestResponse(buffer.CorrelationId)
                    {
                        data = "ResponseONE"
                    }.ToJson()
                });
                buffer.OnResponse(null, new ReceivedEventArgs()
                {
                    Channel = $"Response:{buffer.CorrelationId}",
                    CorrelationId = buffer.CorrelationId,
                    MessageType = $"{typeof(TestResponse)}",
                    Value = new TestResponse(buffer.CorrelationId)
                    {
                        data = "ResponseTWO"
                    }.ToJson()
                });
            }).Verifiable();
            _messaging.Setup(x => x.UnsubscribeResponseAsync(buffer.CorrelationId)).Verifiable();

            // Act
            var result = await buffer.SendRequestAsync();

            // Assert
            Assert.Equal("ResponseONEResponseTWO", result.data);
            _messaging.Verify();
        }


        /// <summary>
        /// Send a TestRequest and verify the content of TestResponse.
        /// The content of TestResponse is computed in Aggregate method.
        /// </summary>
        [Fact]
        public async void ProduceAggregateResponse_WhenTimeout()
        {
            // Arrange
            buffer = new TestRequestBuffer(_serviceProvider.Object);
            _messaging.Setup(x => x.SubscribeResponseAsync(buffer.CorrelationId, buffer.OnResponse)).Verifiable();
            _messaging.Setup(x => x.PublishRequestAsync(buffer.CorrelationId, buffer.Request)).ReturnsAsync(2).Callback(async () =>
            {
                await Task.Delay(50);
                buffer.OnResponse(null, new ReceivedEventArgs()
                {
                    Channel = $"Response:{buffer.CorrelationId}",
                    CorrelationId = buffer.CorrelationId,
                    MessageType = $"{typeof(TestResponse)}",
                    Value = new TestResponse(buffer.CorrelationId)
                    {
                        data = "ResponseONE"
                    }.ToJson()
                });
                await Task.Delay(500); // DELAY THE RESPONSE AFTER THE TIMEOUT (timeout at 300 ms)
                buffer.OnResponse(null, new ReceivedEventArgs()
                {
                    Channel = $"Response:{buffer.CorrelationId}",
                    CorrelationId = buffer.CorrelationId,
                    MessageType = $"{typeof(TestResponse)}",
                    Value = new TestResponse(buffer.CorrelationId)
                    {
                        data = "ResponseTWO"
                    }.ToJson()
                });
            }).Verifiable();
            _messaging.Setup(x => x.UnsubscribeResponseAsync(buffer.CorrelationId)).Verifiable();

            // Act
            var result = await buffer.SendRequestAsync();

            // Assert
            Assert.Equal("ResponseONE", result.data);
            _messaging.Verify();
        }
    }
}
