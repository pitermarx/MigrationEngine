using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MigrationEngine.Implementations.Null;
using MigrationEngine.Interfaces;

namespace MigrationEngine
{
    /// <summary>
    /// The <see cref="MigrationEngine"/> runs each migration and adds it to the journal
    /// The Engine logs the execution time for each migration and returns the total time spent
    /// </summary>
    public class MigrationEngine
    {
        private readonly IDatabase database;
        private readonly ILogger log;

        public MigrationEngine(IDatabase database, ILogger log)
        {
            this.database = database;
            this.log = log ?? NullLogger.Instance;
        }

                public async Task<TimeSpan> DryRun<T>(IEnumerable<IMigration<T>> migrations, IJournal<T> journal = null, CancellationToken ct = default)
            where T : IJournalEntry
        {
            using (var connection = await database.OpenConnection(true))
            {
                return await MigrateInternal(new DryRunDatabase(database.Name, connection), migrations, journal, ct);
            }
        }

        public async Task<TimeSpan> Migrate<T>(IEnumerable<IMigration<T>> migrations, IJournal<T> journal = null, CancellationToken ct = default)
            where T : IJournalEntry
        {
            return await MigrateInternal(database, migrations, journal, ct);
        }

        private async Task<TimeSpan> MigrateInternal<T>(IDatabase db, IEnumerable<IMigration<T>> migrations, IJournal<T> journal, CancellationToken ct)
            where T : IJournalEntry
        {
            journal = journal ?? new NullJournal<T>();
            var existingEntries = await EnsureJournal();

            var times = new Dictionary<string, TimeSpan>();
            var sw = new Stopwatch();

            foreach (var mig in migrations.Where(m => m.ShouldRun(existingEntries)))
            {
                try
                {
                    sw.Start();
                    await mig.Run(db, journal, ct);
                    sw.Stop();

                    times[mig.Name] = sw.Elapsed;
                    log.DebugFormat("{0} ({1}) {2}", db.Name, sw.Elapsed, mig.Name);

                    sw.Reset();
                }
                catch
                {
                    log.ErrorFormat("Failed migration on script {0}", mig.Name);
                    throw;
                }
            }

            return TimeSpan.FromSeconds(times.Sum(t => t.Value.TotalSeconds));

            async Task<IReadOnlyList<T>> EnsureJournal()
            {
                using (var con = await db.OpenConnection())
                {
                    log.InfoFormat("{0} -> Getting journal entries", db.Name);
                    return await journal.EnsureJournal(con, ct);
                }
            }
        }
    }
}
