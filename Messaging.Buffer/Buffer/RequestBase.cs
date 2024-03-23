using Messaging.Buffer.Attributes;
using Newtonsoft.Json;

namespace Messaging.Buffer.Buffer
{
    /// <summary>
    /// Base request
    /// </summary>
    [Request]
    public abstract class RequestBase
    {
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
