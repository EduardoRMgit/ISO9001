namespace ISO9001.GetCustomerFeedbackByCustomerId.BusinessObjects.Interfaces
{
    public interface IGetCustomerFeedbackByCustomerIdInputPort
    {
        Task<IEnumerable<CustomerFeedbackResponse>> HandleAsync(string id, string customerId, DateTime? from, DateTime? end);
    }
}
