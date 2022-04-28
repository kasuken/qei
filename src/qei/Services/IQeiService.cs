namespace qei.Services
{
    public interface IQeiService
    {

        Task<string> Create(string email);

        Task<bool> Add(string database, string key, string value);

        Task<string> Get(string database, string key);

    }
}
