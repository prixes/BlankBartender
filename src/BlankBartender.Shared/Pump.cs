using SQLite;

namespace BlankBartender.Shared
{
    public class Pump
    {
        public int Number { get; set; }
        public short Pin { get; set; }
        public string Value { get; set; }
        public decimal FlowRate { get; set; }
        public decimal? Time { get; set; }
    }
}
