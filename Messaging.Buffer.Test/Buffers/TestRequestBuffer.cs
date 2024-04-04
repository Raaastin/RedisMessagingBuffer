using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Messaging.Buffer.Buffer;
using Microsoft.Extensions.Logging;

namespace Messaging.Buffer.Test.Buffers
{
    /// <summary>
    /// Simple empty Request for test
    /// </summary>
    public class TestRequest : RequestBase
    {

    }

    /// <summary>
    /// Simple empty response
    /// </summary>
    public class TestResponse : ResponseBase
    {
        public string data;
        public TestResponse(string correlationId) : base(correlationId)
        {
        }
    }

    /// <summary>
    /// Simple buffer
    /// </summary>
    public class TestRequestBuffer : RequestBufferBase<TestRequest, TestResponse>
    {
        public TestRequestBuffer(IMessaging messaging, TestRequest request, ILogger<RequestBufferBase<TestRequest, TestResponse>> logger) : base(messaging, request, logger)
        {
        }

        public TestRequestBuffer(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        /// <summary>
        /// Concatenate all response data.
        /// </summary>
        /// <returns></returns>
        protected override TestResponse Aggregate()
        {
            var result = new TestResponse(CorrelationId);
            ResponseCollection.ForEach(response =>
            {
                result.data += response.data;
            });
            return result;
        }
    }

}
