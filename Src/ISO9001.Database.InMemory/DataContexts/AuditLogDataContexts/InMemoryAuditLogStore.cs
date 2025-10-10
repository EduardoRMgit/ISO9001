namespace ISO9001.Database.InMemory.DataContexts.AuditLogDataContexts
{
    internal class InMemoryAuditLogStore
    {
        public List<Entities.AuditLog> AuditLogs { get; } = new();
        public int CurrentId { get; set; }
    }
}
