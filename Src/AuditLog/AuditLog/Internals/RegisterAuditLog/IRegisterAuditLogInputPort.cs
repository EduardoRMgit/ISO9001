namespace ISO9001.AuditLog.Core.Internals.RegisterAuditLog
{
    public interface IRegisterAuditLogInputPort
    {
        Task HandleAsync(AuditLogDto auditLog);
    }
}
