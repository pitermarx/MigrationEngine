using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace DbUpdateLite.Interfaces
{
    public interface ICommandRunner
    {
        Task<T> RunCommand<T>(Func<DbCommand, Task<T>> action);
    }
}