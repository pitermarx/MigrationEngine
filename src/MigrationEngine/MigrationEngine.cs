using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MigrationEngine.Interfaces;
using MigrationEngine.Options;

namespace MigrationEngine
{
    /// <summary>
    /// The <see cref="MigrationEngine"/> runs each migration and adds it to the journal
    /// The journal can be null.
    /// <see cref="IMigration{T}"/> is skipped if
    ///   - There is any <see cref="IJournalEntry"/> that matches the migration AND
    ///   - The <see cref="MigrationOptions"/> are not set to <see cref="MigrationOptions.RunAlways"/>
    ///
    /// The Engine logs the execution time for each migration and returns the total time spent
    /// </summary>
    public class MigrationEngine
    {
        private readonly IDatabase database;
        private readonly ILogger log;

        public MigrationEngine(IDatabase database, ILogger log = null)
        {
            this.database = database;
            this.log = log ?? NullLogger.Instance;
        }

        public async Task EnsureDatabase(CancellationToken? ct = null)
        {
            if (!await database.Exists(ct))
            {
                await database.Create(ct);
            }
        }

        public async Task<TimeSpan> Migrate<T>(IEnumerable<IMigration<T>> migrations, IJournal<T> journal = null, CancellationToken? ct = null)
            where T : IJournalEntry
        {
            ct = ct ?? CancellationToken.None;
            var existingEntries = await EnsureJournal(journal, ct);

            var times = new Dictionary<string, TimeSpan>();

            var sw = new Stopwatch();
            foreach (var mig in migrations)
            {
                sw.Start();

                var ran = false;
                try
                {
                    using (var con = await database.OpenConnection(mig.Options.UseTransaction))
                    {
                        if (mig.Options.RunAlways || !existingEntries.Any(e => mig.Matches(e)))
                        {
                            var entry = await mig.Run(con, ct);
                            ran = true;

                            if (journal != null)
                            {
                                await journal.Add(con, entry, ct);
                            }
                        }

                        con.Commit();
                    }
                }
                catch
                {
                    log.LogError("Failed migration on script {0}", mig.Name);
                    throw;
                }

                sw.Stop();
                if (ran)
                {
                    times[mig.Name] = sw.Elapsed;
                    log.LogDebug("{0} ({1}) {2}", database.Name, sw.Elapsed, mig.Name);
                }
                sw.Reset();
            }

            return TimeSpan.FromSeconds(times.Sum(t => t.Value.TotalSeconds));
        }

        private async Task<IReadOnlyList<T>> EnsureJournal<T>(IJournal<T> journal, CancellationToken? token = null) where T : IJournalEntry
        {
            if (journal == null)
            {
                return Array.Empty<T>();
            }

            using (var con = await database.OpenConnection())
            {
                log.LogInformation("{0} -> Getting journal entries", database.Name);
                return await journal.EnsureJournal(con, token);
            }
        }
    }
}