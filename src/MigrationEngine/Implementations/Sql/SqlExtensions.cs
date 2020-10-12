using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MigrationEngine.Interfaces;
using MigrationEngine.Core;

namespace MigrationEngine.Implementations.Sql
{
    public static class SqlExtensions
    {
        public static async Task SplitAndRun(this ICommandRunner commandRunner, string sql, CancellationToken token = default)
        {
            foreach (var sqlCommand in SplitStatements(sql))
            {
                if (!string.IsNullOrEmpty(sqlCommand))
                {
                    await commandRunner.RunCommand(cmd => cmd
                        .Set(sqlCommand)
                        .ExecuteNonQueryAsync(token));
                }
            }
        }

        /// <summary>
        /// A naive implementation of a regex that splits scripts by GO statements
        ///    \s+GO\s+|^GO\s+|\s+GO$
        /// If there is a GO inside a comment it might break
        /// </summary>
        public static IEnumerable<string> SplitStatements(string sql) =>
            Regex.Split(sql, @"\s+GO\s+|^GO\s+|\s+GO$", RegexOptions.Multiline);
    }
}