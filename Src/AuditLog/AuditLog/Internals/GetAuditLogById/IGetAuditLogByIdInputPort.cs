namespace ISO9001.AuditLog.Core.Internals.GetAuditLogById
{
    public interface IGetAuditLogByIdInputPort
    {
        Task<AuditLogResponse> HandleAsync(string companyId, int id);
    }
}
