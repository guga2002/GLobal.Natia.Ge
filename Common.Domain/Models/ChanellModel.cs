namespace Common.Domain.Models
{
    public class ChanellModel
    {
        public int ID { get; set; }
        public required string Name { get; set; }
        public List<SourceModel> Sources { get; set; } = new List<SourceModel>();
    }
}
