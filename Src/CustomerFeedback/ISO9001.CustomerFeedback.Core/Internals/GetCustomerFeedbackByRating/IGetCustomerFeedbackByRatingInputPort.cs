namespace ISO9001.GetCustomerFeedbackByRating.BusinessObjects.Interfaces
{
    public interface IGetCustomerFeedbackByRatingInputPort
    {
        Task<IEnumerable<CustomerFeedbackResponse>> HandleAsync(string id, int rating, DateTime? from, DateTime? end);
    }
}
