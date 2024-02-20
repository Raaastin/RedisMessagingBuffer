using Newtonsoft.Json;

namespace Messaging.Buffer.Buffer
{
    /// <summary>
    /// Base request
    /// </summary>
    public abstract class RequestBase
    {
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
