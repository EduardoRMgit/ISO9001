namespace ISO9001.Entities.Models
{
    public class NonConformityModel
    {
        public DateTime ReportedAt { get; set; }
        public string AffectedProcess { get; set; }
        public string Status { get; set; }
        public string Cause { get; set; }
        public List<NonConformityDetailModel> Details { get; set; }
    }
}
