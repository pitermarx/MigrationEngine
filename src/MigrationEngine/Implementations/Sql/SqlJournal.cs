using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using MigrationEngine.Core;
using MigrationEngine.Interfaces;

namespace MigrationEngine.Implementations.Sql
{
    /// <summary>
    /// Implements an <see cref="IJournal{T}"/> for an <see cref="SqlJournalEntry"/>
    /// Given the name and schema of the journal table
    /// </summary>
    public class SqlJournal : IJournal<SqlJournalEntry>
    {
        private readonly string schema;
        private readonly string table;

        public SqlJournal(string schema, string table)
        {
            this.schema = schema ?? throw new ArgumentNullException(nameof(schema));
            this.table = table ?? throw new ArgumentNullException(nameof(table));
        }

        /// <summary>
        /// Override this method to make migrations on the journal
        /// </summary>
        public virtual Task EnsureJournalVersion(CancellationToken? token = null) => Task.CompletedTask;

        /// <summary>
        /// Creates the journal table if it does not exist, and
        /// Returns all journal entries
        /// </summary>
        public async Task<IReadOnlyList<SqlJournalEntry>> EnsureJournal(ICommandRunner commandRunner, CancellationToken? token = null)
        {
            if (!await commandRunner.RunCommand(Exists))
            {
                await commandRunner.RunCommand(CreateTable);
            }
            else
            {
                await EnsureJournalVersion();
            }

            return await commandRunner.RunCommand(ReadAll);

            Task<int> CreateTable(DbCommand cmd)
            {
                return cmd.Set($@"
                    IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'{schema}')
                        Exec('CREATE SCHEMA [{schema}] AUTHORIZATION [dbo]')

                    CREATE TABLE [{schema}].[{table}]
                    (
                        [Id] int identity(1,1) NOT NULL CONSTRAINT [PK_{table}_ID] PRIMARY KEY,
                        [Name] nvarchar(255) NOT NULL UNIQUE,
                        [Checksum] nvarchar(255) NOT NULL,
                        [Applied] datetime NOT NULL
                    )").ExecuteNonQueryAsync(token.OrNone());
            }

            Task<IReadOnlyList<SqlJournalEntry>> ReadAll(DbCommand cmd)
            {
                return cmd
                    .Set($@"
                        SELECT [Name], [Checksum], [Applied]
                        FROM [{schema}].[{table}]
                        ORDER BY [Applied]")
                    .ReadAll(r => new SqlJournalEntry
                    {
                        Name = r.GetString(0),
                        Checksum = r.GetString(1),
                        AppliedAt = r.GetDateTime(2)
                    }, token);
            }

            async Task<bool> Exists(DbCommand cmd)
            {
                var sqlCommand = @"
                    SELECT COUNT(1)
                        FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_NAME = @table
                        AND TABLE_SCHEMA = @schema;";

                var result = (int?)await cmd
                    .Set(sqlCommand, ("@table", table), ("@schema", schema))
                    .ExecuteScalarAsync(token.OrNone());
                return result == 1;
            }
        }

        /// <summary>
        /// Adds a journal entry into the journal table
        /// </summary>
        public Task Add(ICommandRunner commandRunner, SqlJournalEntry entry, CancellationToken? token = null)
        {
            return commandRunner.RunCommand(Insert);

            Task<int> Insert(DbCommand cmd) => cmd
                .Set($@"
                    INSERT INTO [{schema}].[{table}]
                        (Name, Checksum, Applied)
                    VALUES
                        (@name, @checksum, @applied)",
                    ("name", entry.Name),
                    ("applied", entry.AppliedAt),
                    ("checksum", entry.Checksum))
                .ExecuteNonQueryAsync(token.OrNone());
        }

        /// <summary>
        /// Updates a journal entry, matching by name
        /// </summary>
        public Task Update(ICommandRunner commandRunner, SqlJournalEntry entry, CancellationToken? token = null)
        {
            return commandRunner.RunCommand(Update);

            Task<int> Update(DbCommand cmd) => cmd
                .Set($@"
                    UPDATE [{schema}].[{table}]
                        SET Checksum = @checksum, Applied = @applied
                    WHERE Name = @name",
                    ("name", entry.Name),
                    ("applied", entry.AppliedAt),
                    ("checksum", entry.Checksum))
                .ExecuteNonQueryAsync(token.OrNone());
        }
    }
}