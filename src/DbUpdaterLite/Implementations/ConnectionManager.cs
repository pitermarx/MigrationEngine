using System;
using System.Data.Common;
using System.Threading.Tasks;
using DbUpdateLite.Interfaces;

namespace DbUpdateLite.Implementations
{
    /// <summary>
    /// Implementation for an <see cref="IConnectionManager"/> which also implements an <see cref="ICommandRunner"/>
    /// Just a placeholder for both a connection and a transaction
    /// The transaction can be null. If so, the commit will be a no-op
    /// </summary>
    public class ConnectionManager : IConnectionManager
    {
        private readonly DbConnection connection;
        private readonly DbTransaction transaction;
        private readonly int commandTimeout;

        public ConnectionManager(DbConnection connection, DbTransaction transaction, int commandTimeout)
        {
            this.connection = connection;
            this.transaction = transaction;
            this.commandTimeout = commandTimeout;
        }

        public async Task<T> RunCommand<T>(Func<DbCommand, Task<T>> action)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.Transaction = transaction;
                cmd.CommandTimeout = commandTimeout;
                return await action(cmd);
            }
        }

        public void Commit()
        {
            transaction?.Commit();
        }

        public void Dispose()
        {
            transaction?.Dispose();
            connection.Dispose();
        }
    }
}