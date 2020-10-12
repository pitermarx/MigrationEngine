using MigrationEngine.Interfaces;

namespace MigrationEngine.Implementations.Sql.Options
{
    /// <summary>
    /// Options In the <see cref="SqlMigration"/>
    /// </summary>
    public class MigrationOptions
    {
        /// <summary>
        /// True by default. If false the migration wont run inside a transaction
        /// </summary>
        public bool UseTransaction { get; set; } = true;

        /// <summary>
        /// False by default. If True, the migration will run again even if it's in the <see cref="IJournalEntry"/> list
        /// </summary>
        public bool RunAlways { get; set; }
    }
}