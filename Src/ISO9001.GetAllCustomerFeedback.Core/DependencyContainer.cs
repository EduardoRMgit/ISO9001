using ISO9001.GetAllCustomerFeedback.BusinessObjects.Interfaces;
using ISO9001.GetAllCustomerFeedback.Core.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace ISO9001.GetAllCustomerFeedback.Core
{
    public static class DependencyContainer
    {
        public static IServiceCollection AddGetAllCustomerFeedbackCoreServices(this IServiceCollection services)
        {
           

            return services;
        }
    }
}
