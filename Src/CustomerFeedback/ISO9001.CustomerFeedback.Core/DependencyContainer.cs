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
        return services;
    }

}
