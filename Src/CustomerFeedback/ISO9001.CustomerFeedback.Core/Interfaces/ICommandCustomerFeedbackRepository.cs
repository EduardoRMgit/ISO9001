namespace ISO9001.CustomerFeedback.Core.Interfaces;
public interface ICommandCustomerFeedbackRepository
{
    Task RegisterCustomerFeedbackAsync(CustomerFeedbackDto customerFeedbackDto);
    Task SaveChangesAsync();

}
