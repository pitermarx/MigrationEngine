using System;

namespace MigrationEngine.Interfaces
{
    public interface IConnectionManager : ICommandRunner, IDisposable
    {
        void Commit();
    }
}