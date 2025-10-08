using DigitalDoor.Reporting.Entities.Helpers;
using DigitalDoor.Reporting.Entities.Interfaces;
using DigitalDoor.Reporting.Entities.Models;
using DigitalDoor.Reporting.Entities.ValueObjects;
using DigitalDoor.Reporting.Entities.ViewModels;
using ISO9001.GenerateAuditReport.BusinessObjects.Interfaces;
using Org.BouncyCastle.Asn1.X509;

namespace ISO9001.GenerateAuditReport.Core.Handlers
{
    internal class GenerateAuditJsonReportTemplateInputPort(
        IGenerateAuditReportRepository repository,
        IReportsOutputPort outputPort,
        IReportAsBytes reportBytes) : IGenerateAuditReportInputPort
    {
        public async ValueTask GenerateAuditReportAsync(string companyId, string entityId, DateTime? from, DateTime? end)
        {
            DateTime UtcFrom = from != null ? from.Value.Date
                : DateTime.UtcNow.Date.AddDays(-30);

            DateTime UtcEnd = end != null ? end.Value.Date.AddDays(1).AddTicks(-1)
                : DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

            var NonConformityResponses = await repository.GeAllNonConformitiessOrderByReportedAt(companyId, entityId, UtcFrom, UtcEnd);
            var IncidentReportResponses = await repository.GeAllIncidentReportsOrderByReportedAt(companyId, entityId, UtcFrom, UtcEnd);
            var CustomerFeedbackResponses = await repository.GetAllCustomerFeedbacksOrderByReportedAt(companyId, entityId, UtcFrom, UtcEnd);


            Setup reportSetUp = new()
            {
                Page = new Format() { Orientation = Orientation.Portrait, Dimension = PageSize.A4, Background = "white", Padding = new(0, 0, 0, 0), Margin = new(0, 0, 0, 0), Position = new(0, 0, 0, 0) },
                Header = new Section(new Format(PageSize.A4.Width, 257) {Background = "Red" }),
                Body = new Section(new Format(PageSize.A4.Width, 237) { Background = "Green"}),
                Footer = new Section(new Format(PageSize.A4.Width, 20) { Background = "Blue"})
            };


            var data = new List<ColumnData>();

            await outputPort.Handle(reportSetUp, data);

            ReportViewModel reportModel = new ReportViewModel(reportSetUp, data);
            byte[] pdfBytes = await reportBytes.GenerateReport(reportModel);

            string folderPath = @"C:\Reports";
            Directory.CreateDirectory(folderPath);
            string fileName = $"PlainTextReport_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
            string fullPath = Path.Combine(folderPath, fileName);

            await File.WriteAllBytesAsync(fullPath, pdfBytes);
        }
    }
}
