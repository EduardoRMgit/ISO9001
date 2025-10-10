namespace ISO9001.AuditLog.Core.Handlers.RegisterAuditLog
{
    internal class RegisterAuditLogHandler(ICommandAuditLogRepository repository) : IRegisterAuditLogInputPort
    {
        public async Task HandleAsync(AuditLogDto auditLog)
        {
            await repository.RegisterAuditLogAsync(auditLog);
            await repository.SaveChangesAsync();
        }
    }
}
