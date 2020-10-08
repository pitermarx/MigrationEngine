using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MigrationEngine.Core;
using MigrationEngine.Interfaces;
using MigrationEngine.Options;

namespace MigrationEngine.Implementations.Sql
{
    /// <summary>
    /// Implementation for <see cref="IDatabase"/> for an SqlServer database
    /// </summary>
    public class SqlDatabase : IDatabase
    {
        /// <summary>
        /// The database name from the <see cref="options"/>'s connectionString
        /// </summary>
        public string Name { get; }

        private readonly ConnectionOptions options;
        private readonly ILogger log;

        public SqlDatabase(ConnectionOptions options, ILogger log)
        {
            Name = new SqlConnectionStringBuilder(options.ConnectionString).InitialCatalog?.Trim();
            this.options = options;
            this.log = log;

            var cnxString = options.ConnectionString?.Trim();
            if (string.IsNullOrEmpty(cnxString))
                throw new ArgumentNullException(nameof(options.ConnectionString));

            if (string.IsNullOrEmpty(Name))
                throw new ArgumentNullException(nameof(Name));
        }

        /// <summary>
        /// Drops the database if it exists on the server
        /// </summary>
        public async Task Drop(CancellationToken? token = null)
        {
            using (var con = await OpenMasterConnection())
            {
                await con.RunCommand(DropDatabase);
                log.LogInformation("Dropped database {0}", Name);
            }

            Task<int> DropDatabase(DbCommand cmd) => cmd
                .Set($@"
                    ALTER DATABASE [{Name}]
                        SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE [{Name}];")
                .ExecuteNonQueryAsync(token.OrNone());
        }

        /// <summary>
        /// Returns true if the database exists on the server
        /// </summary>
        public async Task<bool> Exists(CancellationToken? token = null)
        {
            using (var con = await OpenMasterConnection())
            {
                return await con.RunCommand(DbExists);
            }

            async Task<bool> DbExists(DbCommand cmd)
            {
                var result = (int?)await cmd
                    .Set(@"
                        SELECT TOP 1
                            CASE
                                WHEN dbid IS NOT NULL
                                THEN 1
                                ELSE 0
                            END
                        FROM sys.sysdatabases
                            WHERE name = @database",
                        ("@database", Name))
                    .ExecuteScalarAsync(token.OrNone());
                return result == 1;
            }
        }

        /// <summary>
        /// Creates the database.
        /// </summary>
        public async Task Create(CancellationToken? token = null)
        {
            using (var con = await OpenMasterConnection())
                await new SqlMigration("Create", $"CREATE DATABASE [{Name}]").Run(con, token);
        }

        /// <summary>
        /// Creates a <see cref="IConnectionManager"/>
        /// </summary>
        /// <param name="withTransaction">when true, creates a transaction for the connection manager</param>
        public Task<IConnectionManager> OpenConnection(bool withTransaction = false)
            => OpenConnection(withTransaction, null);

        /// <summary>
        /// Creates a <see cref="IConnectionManager"/> connected to the "master" database
        /// </summary>
        /// <param name="withTransaction">when true, creates a transaction for the connection manager</param>
        public Task<IConnectionManager> OpenMasterConnection(bool withTransaction = false)
            => OpenConnection(withTransaction, "master");

        private async Task<IConnectionManager> OpenConnection(bool withTransaction, string customCatalog)
        {
            var cnxStr = customCatalog == null
                ? options.ConnectionString
                : new SqlConnectionStringBuilder(options.ConnectionString)
                  { InitialCatalog = customCatalog }.ConnectionString;

            var con = new SqlConnection(cnxStr);

            if (options.LogOutput)
                con.InfoMessage += (o, e) => log.LogDebug(" > " + e.Message);

            await con.OpenAsync();
            var tx = withTransaction ? con.BeginTransaction() : null;

            return new ConnectionManager(con, tx, options.Timeout);
        }
    }
}