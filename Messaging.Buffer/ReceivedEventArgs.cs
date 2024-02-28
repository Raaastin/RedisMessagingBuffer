namespace Messaging.Buffer
{
    /// <summary>
    /// Message received event args
    /// </summary>
    public class ReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Channel of the request
        /// </summary>
        public string Channel;
        /// <summary>
        /// Unique Request identifier.
        /// </summary>
        public string CorrelationId;
        /// <summary>
        /// Request payload (json as string)
        /// </summary>
        public string Value;
        /// <summary>
        /// Message type of the request (Type name as string)
        /// </summary>
        public string MessageType;
    }
}
