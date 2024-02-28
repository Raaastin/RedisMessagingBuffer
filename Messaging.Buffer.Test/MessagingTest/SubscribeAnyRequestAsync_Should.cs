using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Messaging.Buffer.Buffer;
using Messaging.Buffer.Exceptions;
using Messaging.Buffer.Redis;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using StackExchange.Redis;
using Xunit.Sdk;

namespace Messaging.Buffer.Test.MessagingTest
{
    public class SubscribeAnyRequestAsync_Should : MessagingTestBase
    {
        public SubscribeAnyRequestAsync_Should() : base()
        {

        }

        [Fact]
        public async void SubscribeToGenericRequestChannel()
        {
            // Arrange
            var channel = "Request:*:*";
            _redisCollectionMock.Setup(x => x.SubscribeAsync(RedisChannel.Pattern(channel), _service.OnRequest)).Verifiable();
            EventHandler<ReceivedEventArgs> OnRequestTest = (object sender, ReceivedEventArgs e) => { };

            // Act
            await _service.SubscribeAnyRequestAsync(OnRequestTest);

            // Assert
            _redisCollectionMock.Verify();
        }

        [Fact]
        public async void LogError_WhenSubscriptionFails()
        {
            // Arrange
            var channel = "Request:*:*";
            _redisCollectionMock.Setup(x => x.SubscribeAsync(RedisChannel.Pattern(channel), _service.OnRequest)).Throws(new Exception("Subscription fails.")).Verifiable();

            // Act
            await _service.SubscribeAnyRequestAsync((object sender, ReceivedEventArgs e) => { });

            // Assert
            _redisCollectionMock.Verify();

            _loggerMock.Verify(x => x.Log(LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Could not Subscribe to channel {channel}")),
                        It.IsAny<Exception>(),
                        It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public async void ThrowException_WhenSubscribingTwice()
        {
            // Arrange
            var channel = "Request:*:*";
            _redisCollectionMock.Setup(x => x.SubscribeAsync(RedisChannel.Pattern(channel), _service.OnRequest)).Verifiable(Times.Once);
            EventHandler<ReceivedEventArgs> OnRequestTest = (object sender, ReceivedEventArgs e) => { };

            // Act
            await _service.SubscribeAnyRequestAsync(OnRequestTest);
            var exception = await Assert.ThrowsAsync<SubscriptionException>(() => _service.SubscribeAnyRequestAsync(OnRequestTest));

            // Assert
            _redisCollectionMock.Verify();
            Assert.Equal("The subscription for any request is done already.", exception.Message);
        }

        [Fact]
        public async void AllowSubscription_OnSecondeTry()
        {
            // Arrange
            var channel = "Request:*:*";
            _redisCollectionMock.SetupSequence(x => x.SubscribeAsync(RedisChannel.Pattern(channel), _service.OnRequest))
                .Throws(new Exception("Subscription fails."))
                .Returns(Task.CompletedTask);
            EventHandler<ReceivedEventArgs> OnRequestTest = (object sender, ReceivedEventArgs e) => { };

            // Act Once
            await _service.SubscribeAnyRequestAsync(OnRequestTest);
            _loggerMock.Verify(x => x.Log(LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Could not Subscribe to channel {channel}")),
                        It.IsAny<Exception>(),
                        It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);

            // Act Twice
            await _service.SubscribeAnyRequestAsync(OnRequestTest);
            _redisCollectionMock.Verify(x => x.SubscribeAsync(RedisChannel.Pattern(channel), _service.OnRequest), Times.Exactly(2));
        }


        [Fact]
        public async void ThrowException_WhenConflictingWithRequestSubscription()
        {
            // Arrange
            var channel = "Request:*:*";
            _service.RequestDelegateCollection.TryAdd("any", null);
            _redisCollectionMock.Setup(x => x.SubscribeAsync(It.IsAny<RedisChannel>(), It.IsAny<Action<RedisChannel, RedisValue>>())).Verifiable(Times.Never);
            EventHandler<ReceivedEventArgs> OnRequestTest = (object sender, ReceivedEventArgs e) => { };

            // Act
            var exception = await Assert.ThrowsAsync<SubscriptionException>(() => _service.SubscribeAnyRequestAsync(OnRequestTest));

            // Assert
            _redisCollectionMock.Verify();
            Assert.Equal("Conflicting subscription detected. Cannot perform both request subscription and any request subscription at the same time.", exception.Message);
        }
    }
}
