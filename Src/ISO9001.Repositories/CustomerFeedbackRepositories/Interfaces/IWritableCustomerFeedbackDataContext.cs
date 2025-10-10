namespace ISO9001.Repositories.CustomerFeedbackRepositories.Interfaces
{
    public interface IWritableCustomerFeedbackDataContext
    {
        Task AddAsync(Entities.CustomerFeedback customerFeedback);
        Task SaveChangesAsync();
    }
}
