using Messaging.Buffer.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Messaging.Buffer.Redis
{
    /// <summary>
    /// Subscribe and unsubscribe using collection of resi Multiplexer. 
    /// Allow application to support multiple redis connexion.
    /// </summary>
    public class RedisCollection : IRedisCollection
    {
        #region Fields 

        private static Random rng = new Random();
        private List<IConnectionMultiplexer> _redisCluster;
        private IOptionsMonitor<RedisOptions> _redisOptions;
        private ILogger<RedisCollection> _logger;

        #endregion

        #region Ctor

        public RedisCollection(ILogger<RedisCollection> logger, IOptionsMonitor<RedisOptions> redisOptions)
        {
            _redisCluster = new();
            _redisOptions = redisOptions;
            _logger = logger;
            CreateConnectionMultiplexer();
        }

        #endregion

        #region Init

        private void CreateConnectionMultiplexer()
        {
            if (_redisOptions is null)
                throw new ArgumentNullException("Redis option is null");

            foreach (var connexionstring in _redisOptions.CurrentValue.RedisConnexionStrings)
            {
                var options = ConfigurationOptions.Parse(connexionstring);
                options.ClientName = Environment.MachineName; // only known at runtime
                options.AllowAdmin = true;
                var multiplexer = ConnectionMultiplexer.Connect(options);
                _redisCluster.Add(multiplexer);
            }
            _logger.LogInformation($"Created: {_redisCluster.Count} Redis Connexion Multiplexed.");
        }

        #endregion

        #region Public Implementation 
        /// <inheritdoc/>
        public ISubscriber GetSubscriber()
        {
            var random = rng.Next(_redisCluster.Count);
            _logger.LogTrace($"GetSubscriber Id: {random}");
            return _redisCluster[random].GetSubscriber();
        }

        /// <inheritdoc/>
        public async Task SubscribeAsync(RedisChannel channel, Action<RedisChannel, RedisValue> handler)
        {
            var Tasklist = new List<Task>();
            foreach (var redis in _redisCluster)
            {
                redis.GetSubscriber()?.SubscribeAsync(channel, handler); //All request
            }
            await Task.WhenAll();
        }

        /// <inheritdoc/>
        public async Task UnsubscribeAsync(RedisChannel channel)
        {
            var Tasklist = new List<Task>();
            foreach (var redis in _redisCluster)
            {
                redis.GetSubscriber()?.Unsubscribe(channel); //All request
            }
            await Task.WhenAll();
        }

        #endregion
    }
}
