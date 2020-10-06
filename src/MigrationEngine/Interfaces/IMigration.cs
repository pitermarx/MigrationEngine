using System.Threading.Tasks;
using MigrationEngine.Options;

namespace MigrationEngine.Interfaces
{
    public interface IMigration<T> where T : IJournalEntry
    {
        string Name { get; }

        MigrationOptions Options { get; }

        Task<T> Run(ICommandRunner commandRunner);

        bool Matches(T entry);
    }
}