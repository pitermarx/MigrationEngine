# MigrationEngine
Some interfaces and a simple engine to perform db migrations with an SQLServer implementation

I was using [DbUp](http://dbup.github.io/), but I needed something simpler and more customizable, so I built this.

Use if you want. Should be simple enough to just copy the code and modify it as needed


```cs
var database = new SqlDatabase(new ConnectionOptions
{
    ConnectionString =  @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;Initial Catalog=testDb",
});

await database.Drop();

var engine = new MigrationEngine(database);
await engine.EnsureDatabase();

await engine.Migrate(
    new SqlMigration("mig 1", "CREATE TABLE MYTABLE"),
    new SqlMigration("mig 2", "DROP TABLE MYTABLE")
);
```
