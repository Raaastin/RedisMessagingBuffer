using Messaging.Buffer.Buffer;
using Microsoft.Extensions.Logging;

namespace Messaging.Buffer.TestApp.Requests
{
    public class ListResourceRequest : RequestBase
    {
    }

    public class ListResourceResponse : ResponseBase
    {
        public List<string> ResourceList { get; set; } = new();
        public ListResourceResponse(string correlationId) : base(correlationId)
        {
        }
    }

    public class ListResourceRequestBuffer : RequestBufferBase<ListResourceRequest, ListResourceResponse>
    {
        public ListResourceRequestBuffer(IMessaging messaging, ListResourceRequest request, ILogger<ListResourceRequestBuffer> logger) : base(messaging, request, logger)
        {
        }

        protected override ListResourceResponse Aggregate()
        {
            var response = new ListResourceResponse(CorrelationId);

            foreach (var res in ResponseCollection)
            {
                foreach(var item in res.ResourceList)
                {
                    response.ResourceList.Add(res.ResponseServerName + ":" + item);
                }                
            }

            return response;
        }
    }
}
