using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Messaging.Buffer.Buffer;
using Messaging.Buffer.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using StackExchange.Redis;
using Xunit.Sdk;

namespace Messaging.Buffer.Test.MessagingTest
{
    public class TestRequest : RequestBase
    {
        public string key { get; set; } = "value";
    }
    public class TestResponse : ResponseBase
    {
        public TestResponse(string correlationId) : base(correlationId)
        {
        }

        public string key { get; set; } = "value";
    }

    public class MessagingTestBase
    {
        protected Messaging _service;
        protected Mock<IServiceProvider> _serviceProviderMock;
        protected Mock<IRedisCollection> _redisCollectionMock;
        protected Mock<ISubscriber> _subscriberMock;
        protected Mock<ILogger<IMessaging>> _loggerMock;

        public MessagingTestBase()
        {
            _serviceProviderMock = new();
            _loggerMock = new();
            _redisCollectionMock = new();
            _subscriberMock = new();

            _serviceProviderMock.Setup(x => x.GetService(typeof(ILogger<IMessaging>))).Returns(_loggerMock.Object);
            _serviceProviderMock.Setup(x => x.GetService(typeof(IRedisCollection))).Returns(_redisCollectionMock.Object);

            _redisCollectionMock.Setup(x => x.GetSubscriber()).Returns(_subscriberMock.Object);

            _service = new Messaging(_serviceProviderMock.Object);
        }
    }
}
