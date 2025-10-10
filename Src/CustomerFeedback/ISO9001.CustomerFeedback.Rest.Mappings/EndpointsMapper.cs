using ISO9001.Entities.Dtos;
using ISO9001.Entities.Requests;
using ISO9001.GetAllCustomerFeedback.BusinessObjects.Interfaces;
using ISO9001.GetCustomerFeedbackByCustomerId.BusinessObjects.Interfaces;
using ISO9001.GetCustomerFeedbackByEntityId.BusinessObjects.Interfaces;
using ISO9001.GetCustomerFeedbackById.BusinessObjects.Interfaces;
using ISO9001.GetCustomerFeedbackByRating.BusinessObjects.Interfaces;
using ISO9001.Helpers;
using ISO9001.RegisterCustomerFeedback.BusinessObjects.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ISO9001.CustomerFeedback.Rest.Mappings
{
    public static class EndpointsMapper
    {
        public static IEndpointRouteBuilder MapCustomerFeedbackEndpoint(
            this IEndpointRouteBuilder builder)
        {
            builder.MapPost("".CreateEndpoint("CustomerFeedbackEndpoints"),
                async (CustomerFeedbackRequest customerFeedback, IRegisterCustomerFeedbackInputPort inputport) =>
                {
                    await inputport.HandleAsync(new CustomerFeedbackDto(
                        customerFeedback.EntityId,
                        customerFeedback.CompanyId,
                        customerFeedback.CustomerId,
                        customerFeedback.Rating,
                        customerFeedback.Comments,
                        customerFeedback.ReportedAt
                        ));
                    return TypedResults.Created();
                });

            builder.MapGet(("{companyId}/" + "Rating" + "/{rating}").CreateEndpoint("CustomerFeedbackEndpoints"), async (
            string companyId,
            int rating,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? end,
            IGetCustomerFeedbackByRatingInputPort inputPort) =>
                    {
                        var result = await inputPort.HandleAsync(companyId, rating, from, end);
                        return TypedResults.Ok(result);

                    });

            builder.MapGet(("{companyId}/" + "Id" + "/{id}").CreateEndpoint("CustomerFeedbackEndpoints"), async (
            string companyId,
            int id,
            IGetCustomerFeedbackByIdInputPort inputport) =>
                    {
                        var Result = await inputport.HandleAsync(companyId, id);
                        return TypedResults.Ok(Result);
                    });

            builder.MapGet(("{companyId}/" + "Entity" + "/{entityId}").CreateEndpoint("CustomerFeedbackEndpoints"), async (
            string companyId,
            string entityId,
            IGetCustomerFeedbackByEntityIdInputPort inputPort) =>
                    {
                        var result = await inputPort.HandleAsync(companyId, entityId);
                        return TypedResults.Ok(result);

                    });

            builder.MapGet(("{companyId}/" + "Customer" + "/{customerId}").CreateEndpoint("CustomerFeedbackEndpoints"), async (
            string companyId,
            string customerId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? end,
            [FromServices] IGetCustomerFeedbackByCustomerIdInputPort inputPort) =>
                    {
                        var result = await inputPort.HandleAsync(companyId, customerId, from, end);
                        return TypedResults.Ok(result);

                    });

            builder.MapGet("{companyId}/".CreateEndpoint("CustomerFeedbackEndpoints"), async (
            string companyId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? end,
            IGetAllCustomerFeedbackInputPort inputPort) =>
                    {
                        var result = await inputPort.HandleAsync(companyId, from, end);
                        return TypedResults.Ok(result);

                    });

            return builder;
        }
    }

}
