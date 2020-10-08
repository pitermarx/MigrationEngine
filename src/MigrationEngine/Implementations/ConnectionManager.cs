using System;
using System.Data.Common;
using System.Threading.Tasks;
using MigrationEngine.Interfaces;

namespace MigrationEngine.Implementations
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

        /// <summary>
        /// Creates a command
        /// Sets the <see cref="DbCommand.Transaction"/> and <see cref="DbCommand.CommandTimeout"/> from the Connection manager fields
        /// Runs the <see cref="Func{T}"/>, passing the <see cref="DbCommand"/>
        /// Disposes the <see cref="DbCommand"/>
        /// returns the result of the <see cref="Func{T}"/>
        /// </summary>
        public async Task<T> RunCommand<T>(Func<DbCommand, Task<T>> action)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.Transaction = transaction;
                cmd.CommandTimeout = commandTimeout;
                return await action(cmd);
            }
        }

        /// <summary>
        /// Commits the connection's transaction if it exists
        /// </summary>
        public void Commit()
        {
            transaction?.Commit();
        }

        /// <summary>
        /// Disposes the <see cref="DbConnection"/> and the <see cref="DbTransaction"/>
        /// </summary>
        public void Dispose()
        {
            transaction?.Dispose();
            connection.Dispose();
        }
    }
}