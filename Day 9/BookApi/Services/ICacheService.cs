namespace Day_9.Services
{
    public interface ICacheService
    {
        T? Get<T>(string key);
        void Set<T>(string Key, T value);
        void Remove(string Key);
    }
}
