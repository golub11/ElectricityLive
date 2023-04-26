
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace nigo.Utility
{
    public class AppCache : IDistributedCache
    {
        private readonly ConnectionMultiplexer _redis;

        public AppCache(string connectionString)
        {
            _redis = ConnectionMultiplexer.Connect(connectionString+ ",abortConnect=false,connectTimeout=30000,responseTimeout=30000");
        }
        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            var db = _redis.GetDatabase();
            var a = db.Ping(CommandFlags.None);

            return await db.StringGetAsync(key);
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            var db = _redis.GetDatabase();
            await db.StringSetAsync(key, value, options?.AbsoluteExpirationRelativeToNow);
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            // Redis automatically refreshes items when they are accessed, so this method is a no-op
            return Task.CompletedTask;
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key);
        }

        public byte[] Get(string key)
        {
            throw new NotImplementedException();
        }

        public void Refresh(string key)
        {
            throw new NotImplementedException();
        }

        public void Remove(string key)
        {
            throw new NotImplementedException();
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            throw new NotImplementedException();
        }
    }

}
