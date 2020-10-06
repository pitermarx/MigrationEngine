using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DbUpdateLite.Core
{
    internal static class Extensions
    {
        internal static readonly ILogger DefaultLogger = LoggerFactory.Create(c => { }).CreateLogger("Default");
        internal static T Set<T>(this T cmd, string sql, params (string name, object value)[] parameters)
            where T : IDbCommand
        {
            // Can be useful to put here Logger.LogEvent(LogLevel.Debug, sql);

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

        internal static Task<int> Run(this DbCommand cmd, string sql, params (string name, object value)[] parameters)
            => cmd.Set(sql, parameters).ExecuteNonQueryAsync();

        internal static async Task<int?> Scalar(this DbCommand cmd, string sql, params (string name, object value)[] parameters)
            => (int?)await cmd.Set(sql, parameters).ExecuteScalarAsync();

        internal static Task<DbDataReader> GetReader(this DbCommand cmd, string sql, params (string name, object value)[] parameters)
            => cmd.Set(sql, parameters).ExecuteReaderAsync();

        internal static async Task<IReadOnlyList<T>> ReadAll<T>(this Task<DbDataReader> reader, Func<DbDataReader, T> read)
        {
            var list = new List<T>();

            using (var r = await reader)
            {
                while (await r.ReadAsync())
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