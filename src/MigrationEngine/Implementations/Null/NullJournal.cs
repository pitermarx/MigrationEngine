using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MigrationEngine.Interfaces;

namespace MigrationEngine.Implementations.Null
{
    public class NullJournal<T> : IJournal<T> where T : IJournalEntry
    {
        public Task<IReadOnlyList<T>> EnsureJournal(ICommandRunner commandRunner, CancellationToken token = default)
        {
            return Task.FromResult<IReadOnlyList<T>>(Array.Empty<T>());
        }

        public Task Add(ICommandRunner commandRunner, T entry, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        public Task Update(ICommandRunner commandRunner, T entry, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }
    }
}
