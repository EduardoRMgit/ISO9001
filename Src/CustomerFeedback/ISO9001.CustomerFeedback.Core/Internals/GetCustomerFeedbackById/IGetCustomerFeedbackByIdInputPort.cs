namespace ISO9001.GetCustomerFeedbackById.BusinessObjects.Interfaces
{
    public interface IGetCustomerFeedbackByIdInputPort
    {
        Task<CustomerFeedbackResponse> HandleAsync(string companyId, int id);
    }
}
