using System;
using System.Threading.Tasks;
using MigrationEngine.Interfaces;
using MigrationEngine.Options;

namespace MigrationEngine.Implementations.Sql
{
    public class SqlMigration : IMigration<SqlJournalEntry>
    {
        private readonly string sql;
        public string Name { get; }
        public MigrationOptions Options { get; }

        public SqlMigration(string name, string sql, MigrationOptions options = null)
        {
            Name = name;
            Options = options ?? new MigrationOptions();
            this.sql = sql;
        }

        public async Task<SqlJournalEntry> Run(ICommandRunner commandRunner)
        {
            await commandRunner.SplitAndRun(sql);

            return new SqlJournalEntry { AppliedAt = DateTime.Now, Name = Name, Checksum = null };
        }

        public bool Matches(SqlJournalEntry entry) => entry.Name == Name;
    }
}