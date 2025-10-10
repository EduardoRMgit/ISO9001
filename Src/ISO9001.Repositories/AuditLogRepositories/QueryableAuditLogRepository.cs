using ISO9001.AuditLog.Core.Interfaces;
using ISO9001.Entities.Responses;
using ISO9001.Repositories.AuditLogRepositories.Entities;
using ISO9001.Repositories.AuditLogRepositories.Interfaces;

namespace ISO9001.Repositories.AuditLogRepositories;
internal class QueryableAuditLogRepository(IQueryableAuditLogDataContext dataContext) : IQueryableAuditLogRepository
{
    public async Task<IEnumerable<AuditLogResponse>> GetAuditLogsByEntityIdAsync(string id, string entityId,
    DateTime? from, DateTime? end)
    {
        IQueryable<AuditLogReadModel> Query = dataContext.AuditLogs
            .Where(AuditLog => AuditLog.CompanyId == id &&
                        AuditLog.EntityId == entityId &&
                        AuditLog.Timestamp >= from &&
                        AuditLog.Timestamp <= end);

        var AuditLogs = await dataContext.ToListAsync(Query);

        return AuditLogs.Select(AuditLog => new AuditLogResponse(
            AuditLog.LogId,
            AuditLog.EntityId,
            AuditLog.Action,
            AuditLog.PerformedBy,
            AuditLog.Timestamp,
            AuditLog.CreatedAt,
            AuditLog.Details));
    }

    public async Task<IEnumerable<AuditLogResponse>> GetAuditLogsByActionAsync(string id, string action,
    DateTime? from, DateTime? end)
    {
        IQueryable<AuditLogReadModel> Query = dataContext.AuditLogs
            .Where(AuditLog => AuditLog.CompanyId == id &&
                            AuditLog.Action == action &&
                            AuditLog.Timestamp >= from &&
                            AuditLog.Timestamp <= end);

        var AuditLogs = await dataContext.ToListAsync(Query);

        return AuditLogs.Select(AuditLog => new AuditLogResponse(
            AuditLog.LogId,
            AuditLog.EntityId,
            AuditLog.Action,
            AuditLog.PerformedBy,
            AuditLog.Timestamp,
            AuditLog.CreatedAt,
            AuditLog.Details));
    }

    public Task<bool> AuditLogExitsByIdAsync(string companyId, int id)
    {
        var AuditLog = dataContext.AuditLogs.FirstOrDefault
            (AuditLog => AuditLog.CompanyId == companyId &&
                AuditLog.LogId == id);

        return Task.FromResult(AuditLog != null);
    }

    public Task<AuditLogResponse> GetAuditLogByIdAsync(string companyId, int id)
    {
        var AuditLog = dataContext
            .AuditLogs.FirstOrDefault(
            AuditLog => AuditLog.CompanyId == companyId &&
                AuditLog.LogId == id);

        return Task.FromResult(new AuditLogResponse(
            AuditLog.LogId,
            AuditLog.EntityId,
            AuditLog.Action,
            AuditLog.PerformedBy,
            AuditLog.Timestamp,
            AuditLog.CreatedAt,
            AuditLog.Details));
    }

    public async Task<IEnumerable<AuditLogResponse>> GetAllAuditLogsOrderedByIdAscendingAsync(
    string id, DateTime? from, DateTime? end)
    {
        IQueryable<AuditLogReadModel> Query = dataContext.AuditLogs
            .Where(AuditLog => AuditLog.CompanyId == id &&
                            AuditLog.Timestamp >= from &&
                            AuditLog.Timestamp <= end)
            .OrderBy(AuditLog => AuditLog.LogId);

        var AuditLogs = await dataContext.ToListAsync(Query);

        return AuditLogs.Select(AuditLog => new AuditLogResponse(
            AuditLog.LogId,
            AuditLog.EntityId,
            AuditLog.Action,
            AuditLog.PerformedBy,
            AuditLog.Timestamp,
            AuditLog.CreatedAt,
            AuditLog.Details));
    }
}
