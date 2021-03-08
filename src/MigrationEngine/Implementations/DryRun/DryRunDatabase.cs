using System;
using System.Threading;
using System.Threading.Tasks;
using MigrationEngine.Interfaces;

namespace MigrationEngine.Implementations.DryRun
{
    public class DryRunDatabase : IDatabase
    {
        public string Name { get; }

        private readonly IConnectionManager connection;

        public DryRunDatabase(string name, IConnectionManager connection)
        {
            Name = name;
            this.connection = new DryRunConnectionManager(connection);
        }

        public Task Drop(CancellationToken token = default) => throw new Exception("Cannot drop in a dry run");

        public Task<bool> Exists(CancellationToken token = default) => Task.FromResult(true);

        public Task Create(CancellationToken token = default) => throw new Exception("Cannot create databases in a dry run");

        public Task<IConnectionManager> OpenConnection(bool withTransaction = false) => Task.FromResult(connection);
    }
}
