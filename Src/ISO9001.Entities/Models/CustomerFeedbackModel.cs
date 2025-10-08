namespace ISO9001.Entities.Models
{
    public class CustomerFeedbackModel
    {
        public string EntityId { get; set; }
        public string CustomerId { get; set; }
        public int Rating { get; set; }
        public DateTime ReportedAt { get; set; }
    }
}
