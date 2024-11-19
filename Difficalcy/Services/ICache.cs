using System.Threading.Tasks;

namespace Difficalcy.Services
{
    public interface ICacheDatabase
    {
        Task<string> GetAsync(string key);

        void Set(string key, string value);
        
        void RemovePrefix(string key);
    }

    public interface ICache
    {
        ICacheDatabase GetDatabase();
    }
}
