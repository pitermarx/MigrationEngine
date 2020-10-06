using System.Data.SqlClient;
using System.Threading.Tasks;
using DbUpdateLite.Implementations.Sql;
using DbUpdateLite.Options;
using NUnit.Framework;

namespace DbUpdaterLite.Tests
{
    public class DbUpdaterLiteTests
    {
        private const string DatabaseName = nameof(DbUpdaterLiteTests);
        private static readonly string LocalDb = $@"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;Initial Catalog={DatabaseName}";
        
        [SetUp]
        public async Task Setup()
        {
            var options = new ConnectionOptions {ConnectionString = LocalDb, LogOutput = true, Timeout = 500};
            var database = new SqlDatabase(options);
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