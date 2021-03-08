using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using MigrationEngine.Implementations.Sql;
using MigrationEngine.Options;
using NUnit.Framework;

namespace MigrationEngine.Tests
{
    public class MigrationEngineTests
    {
        private const string databaseName = nameof(MigrationEngineTests);
        private static readonly string localDb = $@"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;Initial Catalog={databaseName}";

        [SetUp]
        public async Task Setup()
        {
            var options = new ConnectionOptions {ConnectionString = localDb, LogOutput = true, Timeout = 500};
            var log = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            var database = new SqlDatabase(options, log);
            if (await database.Exists())
            {
                await database.Drop();
            }

            await database.Create();
        }

        [Test]
        public void Test1()
        {
            using (var conn = new SqlConnection(localDb))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "Select 1";
                var result = cmd.ExecuteScalar();
                Assert.AreEqual(1, result);
            }
        }
    }
}
