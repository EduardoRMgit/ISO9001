namespace ISO9001.RegisterCustomerFeedback.BusinessObjects.Interfaces
{
    public interface IRegisterCustomerFeedbackInputPort
    {
        Task HandleAsync(CustomerFeedbackDto customerFeedbackDto);
    }
}
