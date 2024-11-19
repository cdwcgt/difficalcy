using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Difficalcy.Services
{
    public class InMemoryCacheDatabase : ICacheDatabase
    {
        private readonly Dictionary<string, string> dictionary = [];

        public Task<string> GetAsync(string key) =>
            Task.FromResult(dictionary.GetValueOrDefault(key, null));

        public void Set(string key, string value) => dictionary[key] = value;
        
        public void RemovePrefix(string prefix)
        {
            var keys = dictionary.Where(kvp => kvp.Value.StartsWith(prefix)).Select(kvp => kvp.Key);
            
            foreach (var key in keys)
            {
                dictionary.Remove(key);
            }
        }
    }

    public class InMemoryCache : ICache
    {
        private readonly InMemoryCacheDatabase _database = new();

        public ICacheDatabase GetDatabase() => _database;
    }
}
