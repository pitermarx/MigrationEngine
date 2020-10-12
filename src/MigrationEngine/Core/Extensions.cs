using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MigrationEngine.Core
{
    internal static class Extensions
    {
        internal static T Set<T>(this T cmd, string sql, params (string name, object value)[] parameters)
            where T : IDbCommand
        {
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql;
            cmd.Parameters.Clear();
            foreach (var (name, value) in parameters)
            {
                var param = cmd.CreateParameter();
                param.ParameterName = name;
                param.Value = value;
                cmd.Parameters.Add(param);
            }
            return cmd;
        }

        internal static async Task<IReadOnlyList<T>> ReadAll<T>(this DbCommand cmd, Func<DbDataReader, T> read, CancellationToken token = default)
        {
            var list = new List<T>();

            using (var r = await cmd.ExecuteReaderAsync(token))
            {
                while (await r.ReadAsync(token))
                {
                    list.Add(read(r));
                }
            }

            return list;
        }

        internal static string GetChecksum(this string content)
        {
            var hash = new StringBuilder();

            using (var md5 = MD5.Create())
            {
                var data = md5.ComputeHash(Encoding.Default.GetBytes(content));
                foreach (var t in data)
                {
                    hash.Append(t.ToString("x2"));
                }
            }

            return hash.ToString();
        }
    }
}