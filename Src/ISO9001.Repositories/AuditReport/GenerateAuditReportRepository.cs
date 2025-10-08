using ISO9001.Entities.Responses;
using ISO9001.GenerateAuditReport.BusinessObjects.Interfaces;
using ISO9001.Repositories.CustomerFeedbackRepositories.Interfaces;
using ISO9001.Repositories.IncidentReportRepositories.Entities;
using ISO9001.Repositories.IncidentReportRepositories.Interfaces;
using ISO9001.Repositories.NonConformityRepositories.Interfaces;

namespace ISO9001.Repositories.AuditReport
{
    internal class GenerateAuditReportRepository(
        IQueryableCustomerFeedbackDataContext customerFeedbackDataContext,
        IQueryableNonConformityDataContext nonConformityDataContext,
        IQueryableIncidentReportDataContext incidentReportDataContext) : IGenerateAuditReportRepository
    {
        public async Task<IEnumerable<IncidentReportResponse>> GeAllIncidentReportsOrderByReportedAt(string companyId,
            string entityId, DateTime? from, DateTime? end)
        {
            var Query = incidentReportDataContext
                .IncidentReports.Where(
                IncidentReport => IncidentReport.CompanyId == companyId &&
                IncidentReport.EntityId == entityId &&
                IncidentReport.ReportedAt >= from &&
                IncidentReport.ReportedAt <= end)
                .OrderBy(IncidentReport => IncidentReport.ReportedAt);

            IEnumerable<IncidentReportReadModel> IncidentReports = await incidentReportDataContext.ToListAsync(Query);

            return IncidentReports.Select(IncidentReport => new IncidentReportResponse
            (
                IncidentReport.EntityId,
                IncidentReport.ReportedAt,
                IncidentReport.UserId,
                IncidentReport.Description,
                IncidentReport.AffectedProcess,
                IncidentReport.Severity,
                IncidentReport.Data
            ));
        }

        public async Task<IEnumerable<NonConformityMaterResponse>> GeAllNonConformitiessOrderByReportedAt(string companyId,
            string entityId, DateTime? from, DateTime? end)
        {
            var NonConformities = await nonConformityDataContext.ToListAsync(
                nonConformityDataContext.NonConformities
                    .Where(NonConformity => NonConformity.CompanyId == companyId &&
                           NonConformity.EntityId == entityId)
                    .OrderBy(NonConformity => NonConformity.ReportedAt));

            var MasterIds = NonConformities
                .Select(NonConformity => NonConformity.Id);

            var Details = await nonConformityDataContext.ToListAsync(
                nonConformityDataContext.NonConformityDetails
                    .Where(Detail => MasterIds.Contains(Detail.NonConformityId)).OrderBy(Detail => Detail.ReportedAt));

            return NonConformities.Select(NC => new NonConformityMaterResponse
            (
                NC.Id,
                NC.EntityId,
                NC.ReportedAt,
                NC.AffectedProcess,
                NC.Cause,
                NC.Status,
                Details.Count()
                ));
        }


        public async Task<IEnumerable<CustomerFeedbackResponse>> GetAllCustomerFeedbacksOrderByReportedAt(string companyId, string entityId,
            DateTime? from, DateTime? end)
        {
            var Query = customerFeedbackDataContext.CustomerFeedbacks
                .Where(CustomerFeedback => CustomerFeedback.CompanyId == companyId &&
                    CustomerFeedback.EntityId == entityId &&
                    CustomerFeedback.ReportedAt >= from &&
                    CustomerFeedback.ReportedAt <= end)
                .OrderBy(CustomerFeedback => CustomerFeedback.ReportedAt);

            var CustomerFeedbacks = await customerFeedbackDataContext.ToListAsync(Query);

            return CustomerFeedbacks.Select(CustomerFeedback => new CustomerFeedbackResponse
            (
                CustomerFeedback.EntityId,
                CustomerFeedback.CustomerId,
                CustomerFeedback.Rating,
                CustomerFeedback.ReportedAt
            )).ToList();
        }
    }
}
