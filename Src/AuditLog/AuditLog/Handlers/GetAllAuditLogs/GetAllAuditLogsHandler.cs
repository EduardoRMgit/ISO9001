namespace ISO9001.AuditLog.Core.Handlers.GetAllAuditLogs
{
    internal class GetAllAuditLogsHandler(IQueryableAuditLogRepository repository) : IGetAllAuditLogsInputPort
    {
        public async Task<IEnumerable<AuditLogResponse>> HandleAsync(string id, DateTime? from, DateTime? end)
        {
            DateTime UtcFrom = from != null ? from.Value.Date
                : DateTime.UtcNow.Date.AddDays(-30);

            DateTime UtcEnd = end != null ? end.Value.Date.AddDays(1).AddTicks(-1)
                : DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

            return await repository.GetAllAuditLogsOrderedByIdAscendingAsync(id, UtcFrom, UtcEnd);
        }
    }
}
