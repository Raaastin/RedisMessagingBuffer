namespace Messaging.Buffer.Service
{
    /// <summary>
    /// Application settings
    /// </summary>
    public class RedisOptions
    {
        /// <summary>
        /// List of redis connexion string
        /// </summary>
        public List<string> RedisConnexionStrings { get; set; }
    }
}
