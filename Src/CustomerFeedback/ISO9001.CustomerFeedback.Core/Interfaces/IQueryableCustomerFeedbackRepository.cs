namespace ISO9001.CustomerFeedback.Core.Interfaces;
public interface IQueryableCustomerFeedbackRepository
{
    Task<IEnumerable<CustomerFeedbackResponse>> GetCustomerFeedbackByRatingAsync(string id, int rating, DateTime? from, DateTime? end);

    Task<CustomerFeedbackResponse> GetCustomerFeedbackByIdAsync(string companyId, int id);

    Task<bool> CustomerFeedbackExists(string companyId, int id);

    Task<IEnumerable<CustomerFeedbackResponse>> GetCustomerFeedbackByEntityId(string id, string entityId);

    Task<IEnumerable<CustomerFeedbackResponse>> GetCustomerFeedbackByCustomerIdAsync(string id, string customerId, DateTime? from, DateTime? end);

    Task<IEnumerable<CustomerFeedbackResponse>> GetAllCustomerFeedbacksAsync(string id, DateTime? from, DateTime? end);
}
