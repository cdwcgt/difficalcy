using System.Threading.Tasks;
using StackExchange.Redis;

namespace Difficalcy.Services
{
    public class RedisCacheDatabase(IDatabase redisDatabase) : ICacheDatabase
    {
        public async Task<string> GetAsync(string key)
        {
            var redisValue = await redisDatabase.StringGetAsync(key);

            if (redisValue.IsNull)
                return null;

            return redisValue;
        }

        public void Set(string key, string value)
        {
            redisDatabase.StringSet(key, value, flags: CommandFlags.FireAndForget);
        }

        public void RemovePrefix(string key)
        {
            string script = @"
            local keys = redis.call('KEYS', ARGV[1])
            for i = 1, #keys, 1 do
                redis.call('DEL', keys[i])
            end
            return #keys";
            
            redisDatabase.ScriptEvaluate(script, values: [key]);
        }
    }

    public class RedisCache(IConnectionMultiplexer redis) : ICache
    {
        private readonly IConnectionMultiplexer _redis = redis;

        public ICacheDatabase GetDatabase()
        {
            return new RedisCacheDatabase(_redis.GetDatabase());
        }
    }
}
