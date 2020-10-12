using System.Threading;
using System.Threading.Tasks;

namespace MigrationEngine.Interfaces
{
    public interface IDatabase
    {
        string Name { get; }
        Task<bool> Exists(CancellationToken token = default);
        Task Drop(CancellationToken token = default);
        Task Create(CancellationToken token = default);
        Task<IConnectionManager> OpenConnection(bool withTransaction = false);
    }
}