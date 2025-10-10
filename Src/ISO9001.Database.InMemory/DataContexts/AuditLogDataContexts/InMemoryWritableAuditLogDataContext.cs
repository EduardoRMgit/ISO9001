using ISO9001.Repositories.AuditLogRepositories.Interfaces;

namespace ISO9001.Database.InMemory.DataContexts.AuditLogDataContexts
{
    internal class InMemoryWritableAuditLogDataContext(
        InMemoryAuditLogStore dataContext) : IWritableAuditLogDataContext
    {
        public Task AddAsync(Repositories.AuditLogRepositories.Entities.AuditLog auditLog)
        {
            var Record = new DataContexts.Entities.AuditLog
            {
                Id = ++dataContext.CurrentId,
                CreatedAt = DateTime.UtcNow,
                EntityId = auditLog.EntityId,
                CompanyId = auditLog.CompanyId,
                Action = auditLog.Action,
                PerformedBy = auditLog.PerformedBy,
                Timestamp = auditLog.Timestamp,
                Details = auditLog.Details,
                Data = auditLog.Data
            };

            dataContext.AuditLogs.Add(Record);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync()
        {
            return Task.CompletedTask;
        }
    }
}
