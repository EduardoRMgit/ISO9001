namespace ISO9001.RegisterCustomerFeedback.Core.Handlers
{
    internal class RegisterCustomerFeedbackHandler(
        ICommandCustomerFeedbackRepository repository) : IRegisterCustomerFeedbackInputPort
    {
        public async Task HandleAsync(CustomerFeedbackDto customerFeedbackDto)
        {
            if (customerFeedbackDto.Rating < 1 || customerFeedbackDto.Rating > 5)
            {
                throw new ArgumentOutOfRangeException(nameof(customerFeedbackDto),
                    "Rating must be between 1 and 5.");
            }

            await repository.RegisterCustomerFeedbackAsync(customerFeedbackDto);
            await repository.SaveChangesAsync();
        }
    }
}
