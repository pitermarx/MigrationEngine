using System.Threading.Tasks;
using DbUpdateLite.Options;

namespace DbUpdateLite.Interfaces
{
    public interface IMigration<T> where T : IJournalEntry
    {
        string Name { get; }

        MigrationOptions Options { get; }

        Task<T> Run(ICommandRunner commandRunner);

        bool Matches(T entry);
    }
}