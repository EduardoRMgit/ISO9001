namespace ISO9001.Entities.Responses
{
    public class QualityDashboardResponse(int openNonConformities, int closedNonConformities,
        TimeSpan avarageResolutionDays, int totalFeedbacks, double avarageRating, 
        Dictionary<string, int> incidentsPerOrder, int totalIncidentReports,
        List<MonthlyQualityKpi> monthlyKpis)
    {
        public int OpenNonConformities => openNonConformities;
        public int ClosedNonConformities => closedNonConformities;
        public TimeSpan AvarageResolutionDays => avarageResolutionDays;
        public int TotalFeedbacks => totalFeedbacks;
        public double AvarageRating => avarageRating;
        public Dictionary<string, int> IncidentsPerOrder => incidentsPerOrder;
        public int TotalIncidentReports => totalIncidentReports;
        public List<MonthlyQualityKpi> MonthlyKpis => monthlyKpis;
    }
}
