using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MigrationEngine.Interfaces
{
    public interface IJournal<T> where T : IJournalEntry
    {
        Task<IReadOnlyList<T>> EnsureJournal(ICommandRunner commandRunner, CancellationToken token = default);
        Task Add(ICommandRunner commandRunner, T entry, CancellationToken token = default);
        Task Update(ICommandRunner commandRunner, T entry, CancellationToken token = default);
    }
}