namespace ISO9001.Repositories.AuditLogRepositories.Interfaces
{
    public interface IWritableAuditLogDataContext
    {
        Task AddAsync(Entities.AuditLog auditLog);
        Task SaveChangesAsync();
    }
}

