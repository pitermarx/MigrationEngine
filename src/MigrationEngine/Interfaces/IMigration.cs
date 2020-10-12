using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MigrationEngine.Interfaces
{
    public interface IMigration<T> where T : IJournalEntry
    {
        string Name { get; }

        Task Run(IDatabase db, IJournal<T> journal, CancellationToken token = default);

        bool ShouldRun(IReadOnlyList<T> existingEntries);
    }
}