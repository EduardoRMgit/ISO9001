namespace ISO9001.AuditLog.Core;

public static class DependencyContainer
{
    public static IServiceCollection AddAuditLogCoreServices(this IServiceCollection services)
    {
        services.AddScoped<IRegisterAuditLogInputPort, RegisterAuditLogHandler>();
        services.AddScoped<IGetAuditLogsByEntityIdInputPort, GetAuditLogsByEntityIdHandler>();
        services.AddScoped<IGetAuditLogsByActionInputPort, GetAuditLogsByActionHandler>();
        services.AddScoped<IGetAuditLogByIdInputPort, GetAuditLogByIdHandler>();
        services.AddScoped<IGetAllAuditLogsInputPort, GetAllAuditLogsHandler>();
        return services;
    }
}
