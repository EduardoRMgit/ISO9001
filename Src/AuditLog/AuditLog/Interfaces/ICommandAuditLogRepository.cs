namespace ISO9001.AuditLog.Core.Interfaces
{
    public interface ICommandAuditLogRepository
    {
        Task RegisterAuditLogAsync(AuditLogDto auditLog);
        Task SaveChangesAsync();
    }
}
