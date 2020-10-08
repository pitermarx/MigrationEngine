using System;
using System.Threading;
using System.Threading.Tasks;
using MigrationEngine.Core;
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

        /// <summary>
        /// Runs an sql script after splitting it in commands
        /// Returns an <see cref="SqlJournalEntry"/>
        /// </summary>
        public async Task<SqlJournalEntry> Run(ICommandRunner commandRunner, CancellationToken? token = null)
        {
            await commandRunner.SplitAndRun(sql, token);

            return new SqlJournalEntry { AppliedAt = DateTime.Now, Name = Name, Checksum = sql.GetChecksum() };
        }
        
        public bool Matches(SqlJournalEntry entry) => entry.Name == Name;
    }
}