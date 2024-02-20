using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Messaging.Buffer.Buffer;

namespace Messaging.Buffer.TestApp
{
    public class HelloWorldRequest : RequestBase
    {
    }

    public class HelloWorldResponse : ResponseBase
    {
        public string InstanceResponse { get; set; }
        public HelloWorldResponse(string correlationId) : base(correlationId)
        {
        }
    }

    public class HelloWorldRequestBuffer : RequestBufferBase<HelloWorldRequest, HelloWorldResponse>
    {
        public HelloWorldRequestBuffer(IMessaging messaging, HelloWorldRequest request) : base(messaging, request)
        {
        }

        protected override HelloWorldResponse Aggregate()
        {
            var response = new HelloWorldResponse(CorrelationId)
            {

            };

            foreach (var res in ResponseCollection)
            {
                response.InstanceResponse += $"{res.InstanceResponse}\r\n";
            }

            return response;
        }
    }
}
