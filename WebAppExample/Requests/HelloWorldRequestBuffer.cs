using Messaging.Buffer;
using Messaging.Buffer.Buffer;
using Microsoft.Extensions.Logging;

namespace WebAppExample.Requests
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
        public HelloWorldRequestBuffer(IServiceProvider serviceProvider) : base(serviceProvider)
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
