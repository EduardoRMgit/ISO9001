using ISO9001.CustomerFeedback.Core.Interfaces;
using ISO9001.Entities.Responses;
using ISO9001.Repositories.CustomerFeedbackRepositories.Entities;
using ISO9001.Repositories.CustomerFeedbackRepositories.Interfaces;

namespace ISO9001.Repositories.CustomerFeedbackRepositories
{
    internal class QueryableCustomerFeedbackRepository(IQueryableCustomerFeedbackDataContext dataContext) : IQueryableCustomerFeedbackRepository
    {
        public async Task<IEnumerable<CustomerFeedbackResponse>> GetAllCustomerFeedbacksAsync(
            string id, DateTime? from, DateTime? end)
        {
            IQueryable<CustomerFeedbackReadModel> Query = dataContext.CustomerFeedbacks
                .Where(CustomerFeedback => CustomerFeedback.CompanyId == id &&
                    CustomerFeedback.ReportedAt >= from &&
                    CustomerFeedback.ReportedAt <= end);

            var CustomerFeedbacks = await dataContext.ToListAsync(Query);

            return CustomerFeedbacks.Select(CustomerFeedback =>
            new CustomerFeedbackResponse(
                CustomerFeedback.EntityId,
                CustomerFeedback.CustomerId,
                CustomerFeedback.Rating,
                CustomerFeedback.ReportedAt));
        }

        public async Task<IEnumerable<CustomerFeedbackResponse>> GetCustomerFeedbackByCustomerIdAsync
        (string id, string customerId, DateTime? from, DateTime? end)
        {
            IQueryable<CustomerFeedbackReadModel> Query = dataContext.CustomerFeedbacks
                .Where(CustomerFeedback => CustomerFeedback.CompanyId == id &&
                CustomerFeedback.CustomerId == customerId &&
                CustomerFeedback.ReportedAt >= from &&
                CustomerFeedback.ReportedAt <= end);

            var CustomerFeedbacks = await dataContext.ToListAsync(Query);

            return CustomerFeedbacks.Select(CustomerFeedback =>
            new CustomerFeedbackResponse(
                CustomerFeedback.EntityId,
                CustomerFeedback.CustomerId,
                CustomerFeedback.Rating,
                CustomerFeedback.ReportedAt));

        }

        public async Task<IEnumerable<CustomerFeedbackResponse>> GetCustomerFeedbackByEntityId(string id, string entityId)
        {
            IQueryable<CustomerFeedbackReadModel> Query =
                dataContext.CustomerFeedbacks.Where(CustomerFeedback => CustomerFeedback.CompanyId == id &&
                CustomerFeedback.EntityId == entityId);

            var CustomerFeedbacks = await dataContext.ToListAsync(Query);

            return CustomerFeedbacks.Select(CustomerFeedback =>
            new CustomerFeedbackResponse(
                CustomerFeedback.EntityId,
                CustomerFeedback.CustomerId,
                CustomerFeedback.Rating,
                CustomerFeedback.ReportedAt));
        }

        public Task<bool> CustomerFeedbackExists(string companyId, int id)
        {
            var CustomerFeedback = dataContext.CustomerFeedbacks
                .FirstOrDefault(CustomerFeedback => CustomerFeedback.CompanyId == companyId &&
                CustomerFeedback.Id == id);

            return Task.FromResult(CustomerFeedback != null);
        }

        public Task<CustomerFeedbackResponse> GetCustomerFeedbackByIdAsync(string companyId, int id)
        {
            var CustomerFeedback = dataContext.CustomerFeedbacks
                .FirstOrDefault(CustomerFeedback => CustomerFeedback.CompanyId == companyId &&
                CustomerFeedback.Id == id);

            return Task.FromResult(new CustomerFeedbackResponse(
                CustomerFeedback.EntityId,
                CustomerFeedback.CustomerId,
                CustomerFeedback.Rating,
                CustomerFeedback.ReportedAt));
        }

        public async Task<IEnumerable<CustomerFeedbackResponse>> GetCustomerFeedbackByRatingAsync(string id, int rating, DateTime? from, DateTime? end)
        {

            IQueryable<CustomerFeedbackReadModel> Query = dataContext.CustomerFeedbacks
                .Where(CustomerFeedback => CustomerFeedback.CompanyId == id &&
                CustomerFeedback.Rating == rating &&
                CustomerFeedback.ReportedAt >= from &&
                CustomerFeedback.ReportedAt <= end);

            var CustomerFeedbacks = await dataContext.ToListAsync(Query);

            return CustomerFeedbacks.Select(CustomerFeedback =>
            new CustomerFeedbackResponse(
                CustomerFeedback.EntityId,
                CustomerFeedback.CustomerId,
                CustomerFeedback.Rating,
                CustomerFeedback.ReportedAt));

        }
    }
}
