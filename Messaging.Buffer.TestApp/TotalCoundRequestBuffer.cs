using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Messaging.Buffer.Buffer;

namespace Messaging.Buffer.TestApp
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
    public class TotalCoundRequestBuffer : RequestBufferBase<TotalCountRequest, TotalCountResponse>
    {
        public TotalCoundRequestBuffer(IMessaging messaging, TotalCountRequest request) : base(messaging, request)
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
