namespace ISO9001.GetCustomerFeedbackByCustomerId.Core.Handlers
{
    internal class GetCustomerFeedbackByCustomerIdHandler
        (IQueryableCustomerFeedbackRepository repository) : IGetCustomerFeedbackByCustomerIdInputPort
    {
        public async Task<IEnumerable<CustomerFeedbackResponse>> HandleAsync(string id, string customerId, DateTime? from, DateTime? end)
        {
            DateTime UtcFrom = from != null ? from.Value.Date
                : DateTime.UtcNow.Date.AddDays(-30);

            DateTime UtcEnd = end != null ? end.Value.Date.AddDays(1).AddTicks(-1)
                : DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

            return await repository.GetCustomerFeedbackByCustomerIdAsync(id, customerId, UtcFrom, UtcEnd);
        }
    }
}
