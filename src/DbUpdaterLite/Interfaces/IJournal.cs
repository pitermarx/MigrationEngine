using System.Collections.Generic;
using System.Threading.Tasks;

namespace DbUpdateLite.Interfaces
{
    public interface IJournal<T> where T : IJournalEntry
    {
        Task<IReadOnlyList<T>> EnsureJournal(ICommandRunner commandRunner);
        Task Add(ICommandRunner commandRunner, T entry);
        Task Update(ICommandRunner commandRunner, T entry);
    }
}