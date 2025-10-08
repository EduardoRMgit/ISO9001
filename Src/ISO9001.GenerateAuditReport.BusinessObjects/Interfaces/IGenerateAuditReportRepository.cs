using ISO9001.Entities.Models;
using ISO9001.Entities.Responses;

namespace ISO9001.GenerateAuditReport.BusinessObjects.Interfaces
{
    public interface IGenerateAuditReportRepository
    {
        Task<IEnumerable<NonConformityMaterResponse>> GeAllNonConformitiessOrderByReportedAt(string companyId, string entityId, DateTime? from, DateTime? end);
        Task<IEnumerable<IncidentReportResponse>> GeAllIncidentReportsOrderByReportedAt(string companyId, string entityId, DateTime? from, DateTime? end);
        Task<IEnumerable<CustomerFeedbackResponse>> GetAllCustomerFeedbacksOrderByReportedAt(string companyId, string entityId, DateTime? from, DateTime? end);
    }
}
