using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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
    public class OnRequest_Should : MessagingTestBase
    {
        public OnRequest_Should() : base()
        {

        }

        [Theory]
        [InlineData("singlepath")]
        [InlineData("double:path")]
        [InlineData("This:is:four:path")]
        public void ThrowException_WhenUnexpectedChannel(string channel)
        {
            // Act
            Assert.Throws<Exception>(() => _service.OnRequest(RedisChannel.Pattern(channel), new TestRequest().ToJson()));
        }

        [Fact]
        public void InvokeHandler_WhenFound()
        {
            // Arrange
            var requestId = Guid.NewGuid().ToString();
            var channel = $"Request:{requestId}:{nameof(TestRequest)}";
            var value = new TestRequest().ToJson();

            var handlerInvoked = false;
            var added = _service.RequestDelegateCollection.TryAdd("TestRequest", (string a, TestRequest b) =>
            {
                handlerInvoked = true;
                Assert.Equal(requestId, a);
                Assert.Equal("value", b.key);
            });

            // Act
            _service.OnRequest(RedisChannel.Pattern(channel), value);

            // Assert
            Assert.True(handlerInvoked);
        }

        [Fact]
        public void LogError_WhenHandlingFails()
        {
            // Arrange
            var requestId = Guid.NewGuid().ToString();
            var channel = $"Request:{requestId}:{nameof(TestRequest)}";
            var value = new TestRequest().ToJson();

            var handlerInvoked = false;
            var added = _service.RequestDelegateCollection.TryAdd("TestRequest", (string a, TestRequest b) =>
            {
                throw new Exception("Invoke did not work");
            });

            // Act
            _service.OnRequest(RedisChannel.Pattern(channel), value);

            // Assert
            _loggerMock.Verify(x => x.Log(LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Request could not be handled. Error when calling delegate.")),
                        It.IsAny<Exception>(),
                        It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
            Assert.False(handlerInvoked);
        }

        [Fact]
        public void FireRequestReceived_WhenNoDedicatedDelegate()
        {
            // Arrange
            var requestId = Guid.NewGuid().ToString();
            var channel = $"Request:{requestId}:{nameof(TestRequest)}";
            var value = new TestRequest().ToJson();

            var handlerInvoked = false;
            _service.RequestReceived += (object source, ReceivedEventArgs a) =>
             {
                 handlerInvoked = true;
                 Assert.Equal(channel, a.Channel);
                 Assert.Equal(requestId, a.CorrelationId);
                 Assert.Equal(value, a.Value);
             };

            // Act
            _service.OnRequest(RedisChannel.Pattern(channel), value);

            // Assert
            Assert.True(handlerInvoked);
        }

        [Fact]
        public void LogError_WhenNoHandlerFound()
        {
            // Arrange
            var requestId = Guid.NewGuid().ToString();
            var channel = $"Request:{requestId}:{nameof(TestRequest)}";
            var value = new TestRequest().ToJson();

            var handlerInvoked = false;

            // Act
            _service.OnRequest(RedisChannel.Pattern(channel), value);

            // Assert
            _loggerMock.Verify(x => x.Log(LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Could not find any handler associated to the request. Request not handled.")),
                        null,
                        It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
            Assert.False(handlerInvoked);
        }
    }
}
