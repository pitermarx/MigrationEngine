using System.Threading.Tasks;

namespace MigrationEngine.Interfaces
{
    public interface IDatabase
    {
        string Name { get; }
        Task<bool> Exists();
        Task Drop();
        Task Create();
        Task<IConnectionManager> OpenConnection(bool withTransaction = false);
    }
}