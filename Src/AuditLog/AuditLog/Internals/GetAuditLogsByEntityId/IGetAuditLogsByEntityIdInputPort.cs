namespace ISO9001.AuditLog.Core.Internals.GetAuditLogsByEntityId
{
    public interface IGetAuditLogsByEntityIdInputPort
    {
        Task<IEnumerable<AuditLogResponse>> HandleAsync(string id, string entityId, DateTime? from, DateTime? end);
    }
}
