using ISO9001.AuditEvents.Repositories;
using ISO9001.AuditLog.Core;
using ISO9001.Database.InMemory;
using ISO9001.GenerateAuditReport.IoC;
using ISO9001.GetAllIncidentReports.IoC;
using ISO9001.GetAllNonConformities.IoC;
using ISO9001.GetAuditEvents.IoC;
using ISO9001.GetIncidentReportById.IoC;
using ISO9001.GetNonConformityByAffectedProcess.IoC;
using ISO9001.GetNonConformityByEntityId.IoC;
using ISO9001.GetNonConformityByStatus.IoC;
using ISO9001.GetQualityDashBoard.IoC;
using ISO9001.RegisterIncidentReportIoC;
using ISO9001.RegisterNonConformityDetail.IoC;
using ISO9001.RegisterNonConformityIoC;
using ISO9001.Repositories;
using ISO9001.CustomerFeedback.Core;

namespace ISO9001.WebAPI
{
    public static class Services
    {
        public static WebApplicationBuilder AddISO9001Services(this WebApplicationBuilder builder)
        {
            builder.Services.AddAuditLogCoreServices();
            builder.Services.AddCustomerFeedbackCoreServices();


            builder.Services.AddRegisterIncidentReportServices();
            builder.Services.AddGetAllIncidentReportServices();
            builder.Services.AddGetIncidentReportByIdServices();

            builder.Services.AddRegisterNonConformityServices();
            builder.Services.AddRegisterNonConformityDetailServices();
            builder.Services.AddGetAllNonConformitiesServices();
            builder.Services.AddGetNonConformityByAffectedProcessServices();
            builder.Services.AddGetNonConformityByEntityIdServices();
            builder.Services.AddGetNonConformityByStatusServices();

            builder.Services.AddGetAuditEventsServices();
            builder.Services.AddAuditEventsRepositories();

            builder.Services.AddGetQualityDashBoardServices();
            builder.Services.AddGenerateAuditReportServices();

            builder.Services.AddISO9001Repositories();
            builder.Services.AddDatabaseInMemory();
            return builder;
        }
    }
}
