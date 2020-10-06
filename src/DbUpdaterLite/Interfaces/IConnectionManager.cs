using System;

namespace DbUpdateLite.Interfaces
{
    public interface IConnectionManager : ICommandRunner, IDisposable
    {
        void Commit();
    }
}