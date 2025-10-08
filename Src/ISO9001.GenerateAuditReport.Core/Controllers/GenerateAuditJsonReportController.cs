using DigitalDoor.Reporting.Entities.Interfaces;
using DigitalDoor.Reporting.Entities.ViewModels;
using ISO9001.GenerateAuditReport.BusinessObjects.Interfaces;

namespace ISO9001.GenerateAuditReport.Core.Controllers
{
    internal class GenerateAuditReportController(
        IGenerateAuditReportInputPort inputPort,
        IReportsPresenter presenter) : IGenerateAuditReportController
    {
        public async Task<ReportViewModel> HandleAsync(string companyId, string entityId, DateTime? from, DateTime? end)
        {
            await inputPort.GenerateAuditReportAsync(companyId, entityId, from, end);
            return presenter.Content;
        }
    }
}
