namespace ISO9001.AuditLog.Core.Internals.GetAllAuditLogs
{
    public interface IGetAllAuditLogsInputPort
    {
        Task<IEnumerable<AuditLogResponse>> HandleAsync(string id, DateTime? from, DateTime? end);
    }
}
