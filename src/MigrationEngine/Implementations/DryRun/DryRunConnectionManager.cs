using System;
using System.Data.Common;
using System.Threading.Tasks;
using MigrationEngine.Interfaces;

namespace MigrationEngine.Implementations.DryRun
{
    public class DryRunConnectionManager : IConnectionManager
    {
        private readonly IConnectionManager manager;

        public DryRunConnectionManager(IConnectionManager manager)
        {
            this.manager = manager;
        }

        public Task<T> RunCommand<T>(Func<DbCommand, Task<T>> action) => manager.RunCommand(action);

        public void Commit()
        {
            // NEVER COMMIT IN A DRY RUN
        }

        public void Dispose()
        {
        }
    }
}
