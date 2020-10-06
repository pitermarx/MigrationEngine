using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DbUpdateLite.Core;
using DbUpdateLite.Interfaces;

namespace DbUpdateLite.Implementations.Sql
{
    public static class SqlExtensions
    {
        public static async Task SplitAndRun(this ICommandRunner commandRunner, string sql)
        {
            foreach (var sqlCommand in GetSqlCommands())
            {
                await commandRunner.RunCommand(cmd => cmd.Run(sqlCommand));
            }

            IEnumerable<string> GetSqlCommands() => Regex
                .Split(sql, @"\s+GO\s+|^GO\s+|\s+GO$", RegexOptions.Multiline)
                .Where(s => !string.IsNullOrWhiteSpace(s));
        }
    }
}