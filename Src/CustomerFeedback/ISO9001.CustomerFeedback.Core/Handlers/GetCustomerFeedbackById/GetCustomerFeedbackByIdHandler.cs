namespace ISO9001.GetCustomerFeedbackById.Core.Handlers
{
    internal class GetCustomerFeedbackByIdHandler
        (IQueryableCustomerFeedbackRepository repository) : IGetCustomerFeedbackByIdInputPort
    {
        public async Task<CustomerFeedbackResponse> HandleAsync(string companyId, int id)
        {
            var CustomerFeedbackExists = await repository.CustomerFeedbackExists(companyId, id);

            if (!CustomerFeedbackExists)
            {
                throw new KeyNotFoundException($"CustomerFeedback with Id '{id}' doesn't exist in the company: '{companyId}'");
            }
            else
            {
                return await repository.GetCustomerFeedbackByIdAsync(companyId, id);
            }

        }
    }
}
