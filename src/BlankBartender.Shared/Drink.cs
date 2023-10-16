using SQLite;

namespace BlankBartender.Shared
{
    public class Drink
    {
        public byte Id { get; set; }
        public string Name { get; set; }
        public byte Type { get; set; }
        public Dictionary<string, float> Ingradients { get; set; } = new Dictionary<string, float>();
        public bool IsProcessing { get; set; }
        public List<string> Garnishes { get; set; }
    }
}
