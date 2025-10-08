namespace ISO9001.GenerateAuditReport.BusinessObjects.Interfaces
{
    public interface IGenerateAuditReportInputPort
    {
        ValueTask GenerateAuditReportAsync(string companyId, string entityId, DateTime? from, DateTime? end);
    }
}
