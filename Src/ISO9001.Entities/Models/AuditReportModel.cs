using ISO9001.Entities.Responses;

namespace ISO9001.Entities.Models
{
    public class AuditReportModel
    {
        public IEnumerable<NonConformityModel> NonConformityResponses { get; set; }
        public IEnumerable<IncidentReportModel> IncidentReportResponses { get; set; }
        public IEnumerable<CustomerFeedbackModel> CustomerFeedbackResponses { get; set; }
    }
}
