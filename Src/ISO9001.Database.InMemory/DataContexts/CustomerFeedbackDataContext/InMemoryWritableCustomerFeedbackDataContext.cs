using ISO9001.Repositories.CustomerFeedbackRepositories.Interfaces;

namespace ISO9001.Database.InMemory.DataContexts.CustomerFeedbackDataContext
{
    internal class InMemoryWritableCustomerFeedbackDataContext(
        InMemoryCustomerFeedbackStore dataContext) : IWritableCustomerFeedbackDataContext
    {
        public Task AddAsync(Repositories.CustomerFeedbackRepositories.Entities.CustomerFeedback customerFeedback)
        {
            var Record = new Entities.CustomerFeedback
            {
                Id = ++dataContext.CurrentId,
                EntityId = customerFeedback.EntityId,
                CompanyId = customerFeedback.CompanyId,
                CustomerId = customerFeedback.CustomerId,
                Rating = customerFeedback.Rating,
                Comments = customerFeedback.Comments,
                ReportedAt = customerFeedback.ReportedAt,
                CreatedAt = DateTime.UtcNow
            };

            dataContext.CustomerFeedbacks.Add(Record);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync()
        {
            return Task.CompletedTask;
        }
    }
}
