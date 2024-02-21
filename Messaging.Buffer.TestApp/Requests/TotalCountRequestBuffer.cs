using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Messaging.Buffer.Buffer;
using Microsoft.Extensions.Logging;

namespace Messaging.Buffer.TestApp.Requests
{
    public class TotalCountRequest : RequestBase
    {
        public int InitialCount { get; set; }
    }

    public class TotalCountResponse : ResponseBase
    {
        public int Count { get; set; }
        public TotalCountResponse(string correlationId, int count) : base(correlationId)
        {
            Count = count;
        }
    }

    /// <summary>
    /// Return the total of COUNT of every instance + the initial COUNT provided in the request
    /// </summary>
    public class TotalCountRequestBuffer : RequestBufferBase<TotalCountRequest, TotalCountResponse>
    {
        public TotalCountRequestBuffer(IMessaging messaging, TotalCountRequest request, ILogger<TotalCountRequestBuffer> logger) : base(messaging, request, logger)
        {
        }

        protected override TotalCountResponse Aggregate()
        {
            var response = new TotalCountResponse(CorrelationId, Request.InitialCount);

            foreach (var res in ResponseCollection)
            {
                response.Count += res.Count;
            }

            return response;
        }
    }
}
