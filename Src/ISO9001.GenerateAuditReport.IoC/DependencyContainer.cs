using ISO9001.GenerateAuditReport.Core;
using Microsoft.Extensions.DependencyInjection;

namespace ISO9001.GenerateAuditReport.IoC
{
    public static class DependencyContainer
    {
        public static IServiceCollection AddGenerateAuditReportServices(this IServiceCollection services)
        {
            services.AddGenerateAuditReportCoreServices();
            services.AddReportingPdfServices();

            return services;
        }
    }

}
