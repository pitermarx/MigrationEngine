using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace MigrationEngine.Interfaces
{
    public interface ICommandRunner
    {
        /// <summary>
        /// Provides a command ready to run
        /// </summary>
        Task<T> RunCommand<T>(Func<DbCommand, Task<T>> action);
    }
}