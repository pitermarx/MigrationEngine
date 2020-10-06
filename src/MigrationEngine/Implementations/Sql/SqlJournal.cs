using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using MigrationEngine.Core;
using MigrationEngine.Interfaces;

namespace MigrationEngine.Implementations.Sql
{
    public abstract class SqlJournal<T> : IJournal<T>
        where T : SqlJournalEntry
    {
        private readonly string schema;
        private readonly string table;

        protected SqlJournal(string schema, string table)
        {
            this.schema = schema ?? throw new ArgumentNullException(nameof(schema));
            this.table = table ?? throw new ArgumentNullException(nameof(table));
        }

        protected abstract T Convert(SqlJournalEntry entry);

        public virtual async Task<IReadOnlyList<T>> EnsureJournal(ICommandRunner commandRunner)
        {
            if (!await commandRunner.RunCommand(Exists))
            {
                await commandRunner.RunCommand(CreateTable);
            }

            return await commandRunner.RunCommand(ReadAll);

            Task<int> CreateTable(DbCommand cmd)
            {
                return cmd.Run($@"
                    IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'{schema}')
                        Exec('CREATE SCHEMA [{schema}] AUTHORIZATION [dbo]')

                    CREATE TABLE [{schema}].[{table}]
                    (
                        [Id] int identity(1,1) NOT NULL CONSTRAINT [PK_{table}_ID] PRIMARY KEY,
                        [Name] nvarchar(255) NOT NULL UNIQUE,
                        [Checksum] nvarchar(255) NOT NULL,
                        [Applied] datetime NOT NULL
                    )");
            }

            Task<IReadOnlyList<T>> ReadAll(DbCommand cmd)
            {
                return cmd
                    .GetReader($@"
                        SELECT [Name], [Checksum], [Applied]
                        FROM [{schema}].[{table}]
                        ORDER BY [Applied]")
                    .ReadAll(r => Convert(new SqlJournalEntry
                    {
                        Name = r.GetString(0),
                        Checksum = r.GetString(1),
                        AppliedAt = r.GetDateTime(2)
                    }));
            }

            async Task<bool> Exists(DbCommand cmd)
            {
                var sqlCommand = @"
                    SELECT COUNT(1)
                        FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_NAME = @table
                        AND TABLE_SCHEMA = @schema;";

                var result = await cmd.Scalar(sqlCommand, ("@table", table), ("@schema", schema));
                return result == 1;
            }
        }

        public Task Add(ICommandRunner commandRunner, T entry)
        {
            return commandRunner.RunCommand(Insert);

            Task<int> Insert(DbCommand cmd) => cmd
                .Run($@"
                    INSERT INTO [{schema}].[{table}]
                        (Name, Checksum, Applied)
                    VALUES
                        (@name, @checksum, @applied)",
                    ("name", entry.Name),
                    ("applied", entry.AppliedAt),
                    ("checksum", entry.Checksum)
                );
        }

        public Task Update(ICommandRunner commandRunner, T entry)
        {
            return commandRunner.RunCommand(Update);

            Task<int> Update(DbCommand cmd) => cmd.Run($@"
                UPDATE [{schema}].[{table}]
                    SET Checksum = @checksum, Applied = @applied
                WHERE Name = @name",
                ("name", entry.Name),
                ("applied", DateTime.Now),
                ("checksum", entry.Checksum));
        }
    }
}