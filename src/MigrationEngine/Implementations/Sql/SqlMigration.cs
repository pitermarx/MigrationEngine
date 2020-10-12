using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MigrationEngine.Implementations.Sql.Options;
using MigrationEngine.Interfaces;

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
        public async Task Run(IDatabase db, IJournal<SqlJournalEntry> journal, CancellationToken token = default)
        {
            using (var con = await db.OpenConnection(Options.UseTransaction))
            {
                await con.SplitAndRun(sql, token);
                var entry = new SqlJournalEntry {AppliedAt = DateTime.Now, Name = Name, Checksum = Checksum()};
                await journal.Add(con, entry, token);
                con.Commit();
            }
        }

        public bool ShouldRun(IReadOnlyList<SqlJournalEntry> existingEntries)
        {
            return Options.RunAlways || existingEntries.All(e => e.Name != Name);
        }

        private string Checksum()
        {
            var bytes = Encoding.ASCII.GetBytes(sql);
            var hash = new StringBuilder();
            using (var md5 = MD5.Create())
            {
                foreach (var t in md5.ComputeHash(bytes))
                {
                    hash.Append(t.ToString("x2"));
                }
            }
            return hash.ToString();
        }
    }
}