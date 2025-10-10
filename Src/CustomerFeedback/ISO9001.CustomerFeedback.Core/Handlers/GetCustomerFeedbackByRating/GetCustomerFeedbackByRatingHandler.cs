namespace ISO9001.GetCustomerFeedbackByRating.Core.Handlers
{
    internal class GetCustomerFeedbackByRatingHandler
        (IQueryableCustomerFeedbackRepository repository) : IGetCustomerFeedbackByRatingInputPort
    {
        public async Task<IEnumerable<CustomerFeedbackResponse>> HandleAsync(string id, int rating, DateTime? from, DateTime? end)
        {
            DateTime UtcFrom = from != null ? from.Value.Date
                : DateTime.UtcNow.Date.AddDays(-30);

            DateTime UtcEnd = end != null ? end.Value.Date.AddDays(1).AddTicks(-1)
                : DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

            return await repository.GetCustomerFeedbackByRatingAsync(id, rating, UtcFrom, UtcEnd);
        }
    }
}
