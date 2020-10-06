using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DbUpdateLite.Core;
using DbUpdateLite.Interfaces;
using DbUpdateLite.Options;
using Microsoft.Extensions.Logging;

namespace DbUpdateLite.Implementations.Sql
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

        public SqlDatabase(ConnectionOptions options, ILogger log = null)
        {
            Name = new SqlConnectionStringBuilder(options.ConnectionString).InitialCatalog?.Trim();
            this.options = options;
            this.log = log ?? Extensions.DefaultLogger;

            var cnxString = options.ConnectionString?.Trim();
            if (string.IsNullOrEmpty(cnxString))
                throw new ArgumentNullException(nameof(options.ConnectionString));

            if (string.IsNullOrEmpty(Name))
                throw new ArgumentNullException(nameof(Name));
        }

        /// <summary>
        /// Drops the database if it exists on the server
        /// </summary>
        public async Task Drop()
        {
            if (!await Exists())
            {
                log.LogWarning("Database {0} does not exist. Cannot drop", Name);
                return;
            }

            using (var con = await OpenMasterConnection())
            {
                await con.RunCommand(DropDatabase);
                log.LogInformation("Dropped database {0}", Name);
            }

            Task<int> DropDatabase(DbCommand cmd) => cmd.Run($@"
                ALTER DATABASE [{Name}]
                    SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [{Name}];");
        }

        /// <summary>
        /// Returns true if the database exists on the server
        /// </summary>
        public async Task<bool> Exists()
        {
            using (var con = await OpenConnection(false, "master"))
            {
                return await con.RunCommand(DbExists);
            }

            async Task<bool> DbExists(DbCommand cmd)
            {
                var result = await cmd.Scalar(@"
                    SELECT TOP 1
                        CASE
                            WHEN dbid IS NOT NULL
                            THEN 1
                            ELSE 0
                        END
                    FROM sys.sysdatabases
                        WHERE name = @database",
                    ("@database", Name));
                return result == 1;
            }
        }

        /// <summary>
        /// Creates the database.
        /// </summary>
        public async Task Create()
        {
            var mig = new SqlMigration("Create", $"CREATE DATABASE [{Name}]", new MigrationOptions {UseTransaction = false});
            using (var con = await OpenMasterConnection())
            {
                await mig.Run(con);
            }
        }
        
        public Task<IConnectionManager> OpenConnection(bool withTransaction = false)
            => OpenConnection(withTransaction, Name);

        public Task<IConnectionManager> OpenMasterConnection(bool withTransaction = false)
            => OpenConnection(withTransaction, "master");

        public async Task<IConnectionManager> OpenConnection(bool withTransaction, string customCatalog)
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