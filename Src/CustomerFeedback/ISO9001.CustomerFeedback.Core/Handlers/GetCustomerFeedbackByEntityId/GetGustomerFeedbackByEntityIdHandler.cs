namespace ISO9001.GetCustomerFeedbackByEntityId.Core.Handler
{
    internal class GetGustomerFeedbackByEntityIdHandler(
        IQueryableCustomerFeedbackRepository repository) : IGetCustomerFeedbackByEntityIdInputPort
    {
        public async Task<IEnumerable<CustomerFeedbackResponse>> HandleAsync(string id, string entityId)
        {
            return await repository.GetCustomerFeedbackByEntityId(id, entityId);
        }

    }
}
