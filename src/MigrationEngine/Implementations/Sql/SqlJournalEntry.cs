using System;
using MigrationEngine.Interfaces;

namespace MigrationEngine.Implementations.Sql
{
    /// <summary>
    /// Defines a journal entry in an sql database
    /// </summary>
    public class SqlJournalEntry : IJournalEntry
    {
        public string Name { get; set; }

        public DateTime AppliedAt { get; set; }

        public string Checksum { get; set; }
    }
}