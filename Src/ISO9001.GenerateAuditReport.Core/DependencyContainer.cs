using ISO9001.GenerateAuditReport.BusinessObjects.Interfaces;
using ISO9001.GenerateAuditReport.Core.Controllers;
using ISO9001.GenerateAuditReport.Core.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace ISO9001.GenerateAuditReport.Core
{
    public static class DependencyContainer
    {
        public static IServiceCollection AddGenerateAuditReportCoreServices(this IServiceCollection services)
        {
            services.AddScoped<IGenerateAuditReportController, GenerateAuditReportController>();
            //services.AddScoped<IGenerateAuditReportInputPort, GenerateAuditReportInputPort>();
            services.AddScoped<IGenerateAuditReportInputPort, GenerateAuditJsonReportTemplateInputPort>();

            services.AddReportingPresenterPdfServices();
            return services;
        }
    }

}
