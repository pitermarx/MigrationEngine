using DbUpdateLite.Implementations.Sql;

namespace  DbUpdateLite.Options
{
    /// <summary>
    /// Options used to create an <see cref="SqlDatabase"/>
    /// </summary>
    public class ConnectionOptions
    {
        public string ConnectionString { get; set; }
        public int Timeout { get; set; }
        public bool LogOutput { get; set; }
    }
}