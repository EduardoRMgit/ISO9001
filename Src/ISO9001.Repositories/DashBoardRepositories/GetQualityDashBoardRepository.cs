using ISO9001.CustomerFeedback.Core.Interfaces;
using ISO9001.Entities.Responses;
using ISO9001.GetAllCustomerFeedback.BusinessObjects.Interfaces;
using ISO9001.GetAllIncidentReports.BusinessObjects.Interfaces;
using ISO9001.GetAllNonConformities.BusinessObjects;
using ISO9001.GetQualityDashBoard.BusinessObjects.Interfaces;
using ISO9001.Repositories.NonConformityRepositories.Interfaces;

namespace ISO9001.Repositories.DashBoardRepositories
{
    internal class GetQualityDashBoardRepository(
        IQueryableCustomerFeedbackRepository getAllCustomerFeedbackRepository,
        IGetAllIncidentReportsRepository getAllIncidentReportRepository,
        IGetAllNonConformitiesRepository getAllNonConformitiesRepository,
        IQueryableNonConformityDataContext nonConformityDataContext) : IGetQualityDashBoardRepository
    {

        public async Task<TimeSpan> GetAverageResolutionDays(string companyId, DateTime? from, DateTime? end)
        {
            var NonConformities = await getAllNonConformitiesRepository.GetAllNonConformitiesAsync(companyId, from, end);
            var NonConformityIds = NonConformities
                .Select(NonConformity => NonConformity.Id)
                .ToList();

            var NonConformityDetails = nonConformityDataContext.NonConformityDetails
                .Where(Detail => NonConformityIds.Contains(Detail.NonConformityId) && 
                    Detail.ReportedAt >= from && Detail.ReportedAt <= end)
                .GroupBy(Detail => Detail.NonConformityId)
                .ToList();

            var AverageDates = new List<TimeSpan>();

            foreach (var group in NonConformityDetails)
            {
                if (group.Count() > 1)
                {
                    var OrderedDates = group
                        .Select(Detail => Detail.ReportedAt)
                        .OrderBy(Date => Date)
                        .ToList();

                    List<TimeSpan> Intervals = new();

                    for (int i = 1; i < OrderedDates.Count; i++)
                    {
                        Intervals.Add(OrderedDates[i] - OrderedDates[i - 1]);
                    }
                    var AverageTicks = Intervals.Average(ts => ts.Ticks);
                    AverageDates.Add(TimeSpan.FromTicks(Convert.ToInt64(AverageTicks)));

                }

            }

            if (!AverageDates.Any())
                return TimeSpan.Zero;

            var AllAverageTicks = AverageDates.Average(ts => ts.Ticks);
            return TimeSpan.FromTicks(Convert.ToInt64(AllAverageTicks));
        }

        public Task<int> GetNonConformitiesCountByStatus(string companyId, string status,
            DateTime? from, DateTime? end) =>
            Task.FromResult(nonConformityDataContext.NonConformities
                .Where(NonConformity =>
                NonConformity.CompanyId == companyId &&
                NonConformity.Status.ToLower() == status.ToLower() &&
                NonConformity.ReportedAt >= from &&
                NonConformity.ReportedAt <= end)
                .Count());


        public Task<int> GetOpenNonConformitiesCount(string companyId, string closedStatus,
            DateTime? from, DateTime? end) =>
            Task.FromResult(nonConformityDataContext.NonConformities
                .Where(NonConformity =>
                NonConformity.CompanyId == companyId &&
                NonConformity.Status.ToLower() != closedStatus.ToLower() &&
                NonConformity.ReportedAt >= from &&
                NonConformity.ReportedAt <= end)
                .Count());

        public async Task<int> GetTotalCustomerFeedbacks(string companyId, DateTime? from, DateTime? end) =>
            (await getAllCustomerFeedbackRepository.GetAllCustomerFeedbacksAsync(companyId, from, end)).Count();

        public async Task<double> GetAverageRatingOfCustomerFeedback(string companyId, DateTime? from, DateTime? end)
        {
            var CustomerFeedbacks = await getAllCustomerFeedbackRepository.
                GetAllCustomerFeedbacksAsync(companyId, from, end);
            if (!CustomerFeedbacks.Any())
                return 0;
            return CustomerFeedbacks.Average(CustomerFeedback => CustomerFeedback.Rating);
        }

        public async Task<int> GetTotalIncidentReports(string companyId, DateTime? from, DateTime? end) =>
            (await getAllIncidentReportRepository.GetAllIncidentReportsAsync(companyId, from, end)).Count();

        public async Task<Dictionary<string, int>> GetIncidentReportsByEntityId(
            string companyId, DateTime? from, DateTime? end)
        {
            var IncidentReports = await getAllIncidentReportRepository.GetAllIncidentReportsAsync
                (companyId, from, end);

            var IncidentReportsByEntityId = IncidentReports
                .GroupBy(IncidentReport => IncidentReport.EntityId)
                .ToDictionary(Group => Group.Key, Group => Group.Count());

            return IncidentReportsByEntityId;
        }

        public async Task<List<MonthlyQualityKpi>> GetMonthlyQualityKpis(string companyId, DateTime? from, DateTime? end)
        {
            var NonConformities = await getAllNonConformitiesRepository.GetAllNonConformitiesAsync(companyId, from, end);
            var NonConformitiesMonthlyKpi =
                NonConformities
                .GroupBy(NonConformity => new
                {
                    Year = NonConformity.ReportedAt.Year,
                    Month = NonConformity.ReportedAt.Month
                })
                .Select(Group => new { Group.Key.Year, Group.Key.Month, NC = Group.Count(), FB = 0 });

            var Feedbacks = await getAllCustomerFeedbackRepository.GetAllCustomerFeedbacksAsync(companyId, from, end);
            var FeedbacksMonthlyKpi =
                Feedbacks
                .GroupBy(Feedback => new
                {
                    Year = Feedback.ReportedAt.Year,
                    Month = Feedback.ReportedAt.Month
                })
                .Select(Group => new { Group.Key.Year, Group.Key.Month, NC = 0, FB = Group.Count() });

            var MonthlyKpis = NonConformitiesMonthlyKpi
                .Concat(FeedbacksMonthlyKpi)
                .GroupBy(MonthlyItem => new { MonthlyItem.Year, MonthlyItem.Month })
                .Select(Group => new MonthlyQualityKpi(
                    Group.Key.Year.ToString(), Group.Key.Month.ToString("D2"),
                    Group.Sum(MonthlyItem => MonthlyItem.NC), Group.Sum(MonthlyItem => MonthlyItem.FB)
                    ))
                    .OrderByDescending(kpi => int.Parse(kpi.Year))
                    .ThenByDescending(kpi => int.Parse(kpi.Month)); ;

            return MonthlyKpis.ToList();
        }
    }
}
