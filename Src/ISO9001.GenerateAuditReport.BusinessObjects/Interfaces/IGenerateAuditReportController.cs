using DigitalDoor.Reporting.Entities.ViewModels;

namespace ISO9001.GenerateAuditReport.BusinessObjects.Interfaces
{
    public interface IGenerateAuditReportController
    {
        Task<ReportViewModel> HandleAsync(string companyId, string entityId, DateTime? from, DateTime? end);
    }
}
