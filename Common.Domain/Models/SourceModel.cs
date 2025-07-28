namespace Common.Domain.Models
{
    public class SourceModel
    {
        public int Id { get; set; }

        public required string ChanellFormat { get; set; }

        public bool Status { get; set; }

        public int ChanellId { get; set; }

        public int? Reciever_ID { get; set; }


        public string EMR { get; set; } = "Undefined";

        public string sourceName { get; set; } = "Undefined";

        public string card { get; set; } = "Undefined";

        public string port { get; set; } = "Undefined";
    }
}
