using Microsoft.Extensions.Caching.Memory;

namespace Day_9.Services
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public T? Get<T>(string key)
        {
            Console.WriteLine($"Checking cache for key: {key}");

            _cache.TryGetValue(key, out T? value);

            if (value != null)
                Console.WriteLine("Cache HIT");

            else
                Console.WriteLine("Cache MISS");

            return value;
        }

        public void Set<T>(string key, T value)
        {
            _cache.Set(key, value);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }
    }
}