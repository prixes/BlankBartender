
namespace BlankBartender.Shared
{
    public class Drink
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public byte Type { get; set; }
        public Dictionary<string, decimal> Ingradients { get; set; } = new Dictionary<string, decimal>();
        public bool IsProcessing { get; set; }
        public List<string> Garnishes { get; set; }
    }
}
