using System;
using DbUpdateLite.Interfaces;

namespace DbUpdateLite.Implementations.Sql
{
    public class SqlJournalEntry : IJournalEntry
    {
        public string Name { get; set; }

        public DateTime AppliedAt { get; set; }

        public string Checksum { get; set; }
    }
}