using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MigrationEngine.Interfaces
{
    public interface IJournal<T> where T : IJournalEntry
    {
        Task<IReadOnlyList<T>> EnsureJournal(ICommandRunner commandRunner, CancellationToken? token = null);
        Task Add(ICommandRunner commandRunner, T entry, CancellationToken? token = null);
        Task Update(ICommandRunner commandRunner, T entry, CancellationToken? token = null);
    }
}