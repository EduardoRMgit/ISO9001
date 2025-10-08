using ISO9001.GenerateAuditReport.BusinessObjects.Interfaces;
using ISO9001.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ISO9001.GenerateAuditReport.Mappings
{
    public static class EndpointsMapper
    {
        public static IEndpointRouteBuilder MapGenerateAuditReportEndpoint(
            this IEndpointRouteBuilder builder)
        {
            builder.MapGet(
                "{companyId}/{entityId}/" 
                .CreateEndpoint("AuditReportEndpoints"),
                 async (
                string companyId,
                string entityId,
                [FromQuery] DateTime? from,
                [FromQuery] DateTime? end,
                IGenerateAuditReportController controller) =>
            {
                var result = await controller.HandleAsync(companyId, entityId, from, end);
                return TypedResults.Ok(result);

            });

            return builder;

        }
    }

}
