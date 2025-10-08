namespace ISO9001.Entities.Models
{
    public class NonConformityDetailModel
    {
        public DateTime ReportedAt { get; set; }
        public string ReportedBy { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }
}
