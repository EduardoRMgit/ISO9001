using ISO9001.CustomerFeedback.Core.Interfaces;
using ISO9001.Entities.Dtos;
using ISO9001.Repositories.CustomerFeedbackRepositories.Interfaces;

namespace ISO9001.Repositories.CustomerFeedbackRepositories
{
    internal class CommandCustomerFeedbackRepository(
        IWritableCustomerFeedbackDataContext dataContext) : ICommandCustomerFeedbackRepository
    {
        public async Task RegisterCustomerFeedbackAsync(CustomerFeedbackDto customerFeedbackDto)
        {
            var NewCustomerFeedback = new Entities.CustomerFeedback
            {
                EntityId = customerFeedbackDto.EntityId,
                CompanyId = customerFeedbackDto.CompanyId,
                CustomerId = customerFeedbackDto.CustomerId,
                Rating = customerFeedbackDto.Rating,
                Comments = customerFeedbackDto.Comments,
                ReportedAt = customerFeedbackDto.ReportedAt
            };

            await dataContext.AddAsync(NewCustomerFeedback);
        }

        public async Task SaveChangesAsync()
        {
            await dataContext.SaveChangesAsync();
        }
    }
}
