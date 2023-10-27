namespace BlankBartender.WebApi.External.Models
{
    public class DetectResultGroup
    {
        public int Length { get; set; }
        public int Count { get; set; }
        public List<DetectResult> Results { get; set; }

        public new string ToString => string.Join("\n", Results.ToList().Select(x => $"{x.Name} {x.Prop}"));
    }
}