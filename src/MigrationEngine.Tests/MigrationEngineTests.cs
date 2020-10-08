using System.Data.SqlClient;
using System.Threading.Tasks;
using MigrationEngine.Implementations.Sql;
using MigrationEngine.Options;
using NUnit.Framework;

namespace MigrationEngine.Tests
{
    public class MigrationEngineTests
    {
        private const string DatabaseName = nameof(MigrationEngineTests);
        private static readonly string LocalDb = $@"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;Initial Catalog={DatabaseName}";

        [SetUp]
        public async Task Setup()
        {
            var options = new ConnectionOptions {ConnectionString = LocalDb, LogOutput = true, Timeout = 500};
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
            using (var conn = new SqlConnection(LocalDb))
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