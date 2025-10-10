namespace ISO9001.GetAllCustomerFeedback.BusinessObjects.Interfaces
{
    public interface IGetAllCustomerFeedbackInputPort
    {
        Task<IEnumerable<CustomerFeedbackResponse>> HandleAsync(string id, DateTime? from, DateTime? end);
    }
}
