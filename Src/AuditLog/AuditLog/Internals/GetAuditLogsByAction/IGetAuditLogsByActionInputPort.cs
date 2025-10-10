namespace ISO9001.AuditLog.Core.Internals.GetAuditLogsByAction
{
    public interface IGetAuditLogsByActionInputPort
    {
        Task<IEnumerable<AuditLogResponse>> HandleAsync(string id, string action, DateTime? from, DateTime? end);
    }
}
