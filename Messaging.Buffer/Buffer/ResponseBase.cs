using Messaging.Buffer.Attributes;
using Newtonsoft.Json;

namespace Messaging.Buffer.Buffer
{
    /// <summary>
    /// Base response
    /// </summary>
    [Response]
    public abstract class ResponseBase
    {
        public string CorrelationId { get; set; }
        public string ResponseServerName { get; set; }

        public ResponseBase(string correlationId)
        {
            CorrelationId = correlationId;
            ResponseServerName = Environment.MachineName;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
