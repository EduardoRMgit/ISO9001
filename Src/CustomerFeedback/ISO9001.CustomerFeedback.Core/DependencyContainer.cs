using ISO9001.GetCustomerFeedbackByCustomerId.Core.Handlers;

namespace ISO9001.CustomerFeedback.Core;

public static class DependencyContainer
{
    public static IServiceCollection AddCustomerFeedbackCoreServices(
          this IServiceCollection services)
    {
        services.AddScoped<IRegisterCustomerFeedbackInputPort, RegisterCustomerFeedbackHandler>();
        services.AddScoped<IGetCustomerFeedbackByRatingInputPort, GetCustomerFeedbackByRatingHandler>();
        services.AddScoped<IGetCustomerFeedbackByIdInputPort, GetCustomerFeedbackByIdHandler>();
        services.AddScoped<IGetCustomerFeedbackByEntityIdInputPort, GetGustomerFeedbackByEntityIdHandler>();
        services.AddScoped<IGetAllCustomerFeedbackInputPort, GetAllCustomerFeedbackHandler>();
        services.AddScoped<IGetCustomerFeedbackByCustomerIdInputPort, GetCustomerFeedbackByCustomerIdHandler>();
        return services;
    }

}
