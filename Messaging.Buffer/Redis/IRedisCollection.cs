using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Messaging.Buffer.Redis
{
    /// <summary>
    /// Subscribe and unsubscribe
    /// </summary>
    public interface IRedisCollection
    {
        /// <summary>
        /// Get a subscriber
        /// </summary>
        /// <returns></returns>
        ISubscriber GetSubscriber();

        /// <summary>
        /// Subscribe
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        Task SubscribeAsync(RedisChannel channel, Action<RedisChannel, RedisValue> handler);

        /// <summary>
        /// Unsubscribe
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        Task UnsubscribeAsync(RedisChannel channel);
    }
}
