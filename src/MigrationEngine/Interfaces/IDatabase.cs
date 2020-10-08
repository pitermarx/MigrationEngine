using System.Threading;
using System.Threading.Tasks;

namespace MigrationEngine.Interfaces
{
    public interface IDatabase
    {
        string Name { get; }
        Task<bool> Exists(CancellationToken? token = null);
        Task Drop(CancellationToken? token = null);
        Task Create(CancellationToken? token = null);
        Task<IConnectionManager> OpenConnection(bool withTransaction = false);
    }
}