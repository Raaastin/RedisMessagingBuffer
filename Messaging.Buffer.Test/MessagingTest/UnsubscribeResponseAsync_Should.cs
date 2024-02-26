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
using Moq;
using Newtonsoft.Json;
using StackExchange.Redis;
using Xunit.Sdk;

namespace Messaging.Buffer.Test.MessagingTest
{
    public class UnsubscribeResponseAsync_Should : MessagingTestBase
    {
        public UnsubscribeResponseAsync_Should() : base()
        {

        }

        [Fact]
        public async void UnsubscribeFromResponseChannel()
        {
            // Arrange
            var requestId = Guid.NewGuid().ToString();
            var channel = $"Response:{requestId}";
            _redisCollectionMock.Setup(x => x.UnsubscribeAsync(RedisChannel.Pattern(channel))).Verifiable();
            _service.ResponseDelegateCollection.TryAdd($"{requestId}", () => { });

            // Act
            await _service.UnsubscribeResponseAsync(requestId);

            // Assert
            _redisCollectionMock.Verify();
            Assert.Empty(_service.ResponseDelegateCollection);
        }

        [Fact]
        public async void LogError_WhenUnsubFails()
        {
            // Arrange
            var requestId = Guid.NewGuid().ToString();
            var channel = $"Response:{requestId}";
            _redisCollectionMock.Setup(x => x.UnsubscribeAsync(RedisChannel.Pattern(channel))).Throws(new Exception("Unsub fails.")).Verifiable();
            _service.ResponseDelegateCollection.TryAdd($"{requestId}", () => { });

            // Act
            await _service.UnsubscribeResponseAsync(requestId);

            // Assert
            _redisCollectionMock.Verify();
            Assert.NotEmpty(_service.ResponseDelegateCollection);

            _loggerMock.Verify(x => x.Log(LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Could not Unsubscribe to channel {channel}")),
                        It.IsAny<Exception>(),
                        It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
