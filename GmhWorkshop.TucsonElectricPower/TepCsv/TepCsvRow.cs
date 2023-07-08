using CsvHelper.Configuration.Attributes;

namespace Pw02.TepCsv
{
    public class TepCsvRow
    {
        [Name("TYPE")]
        public string EntryType { get; set; }
        [Name("DATE")]
        public DateOnly Date { get; set; }
        [Name("START TIME")]
        public TimeOnly StartTime { get; set; }
        [Name("END TIME")]
        public TimeOnly EndTime { get; set; }
        [Name("USAGE")]
        public decimal Usage { get; set; }
        [Name("UNITS")]
        public string Units { get; set; }
        [Name("NOTES")]
        public string Notes { get; set; }

    }
}
