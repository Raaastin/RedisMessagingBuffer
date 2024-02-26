using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Messaging.Buffer.Buffer;
using Messaging.Buffer.Redis;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Moq;
using Newtonsoft.Json;
using StackExchange.Redis;
using Xunit.Sdk;

namespace Messaging.Buffer.Test.MessagingTest
{
    public class SubscribeRequestAsync_Should : MessagingTestBase
    {
        public SubscribeRequestAsync_Should() : base()
        {

        }

        private void TestRequestHandler(string correlationId, TestRequest req)
        {

        }

        [Fact]
        public async void SubscribeToRequestChannel()
        {
            // Arrange
            var channel = $"Request:*:{typeof(TestRequest)}";
            _redisCollectionMock.Setup(x => x.SubscribeAsync(RedisChannel.Pattern(channel), _service.OnRequest)).Verifiable();

            // Act
            await _service.SubscribeRequestAsync<TestRequest>(TestRequestHandler);

            // Assert
            Assert.Single(_service.RequestDelegateCollection);
            Assert.Equal(TestRequestHandler, _service.RequestDelegateCollection[$"{typeof(TestRequest)}"]);
            _redisCollectionMock.Verify();
        }

        [Fact]
        public async void LogError_WhenSubscriptionFails()
        {
            // Arrange
            var channel = $"Request:*:{typeof(TestRequest)}";
            _redisCollectionMock.Setup(x => x.SubscribeAsync(RedisChannel.Pattern(channel), _service.OnRequest)).Throws(new Exception("Subscribe fails.")).Verifiable();

            // Act
            await _service.SubscribeRequestAsync<TestRequest>(TestRequestHandler);

            // Assert
            _redisCollectionMock.Verify();
            Assert.Empty(_service.RequestDelegateCollection);

            _loggerMock.Verify(x => x.Log(LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Could not Subscribe to channel {channel}")),
                        It.IsAny<Exception>(),
                        It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
