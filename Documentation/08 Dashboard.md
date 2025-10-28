# Dashboard de Calidad
Es esencial para ISO 9001, ya que permite monitorear indicadores clave (KPIs) del sistema de gestión de calidad.

### Objetivo del Dashboard de Calidad
Mostrar visualmente en Blazor los siguientes indicadores:

- Número de no conformidades abiertas/cerradas.
- Tiempo promedio de resolución de no conformidades.
- Cantidad de feedbacks recibidos vs. calificación promedio.
- Pedidos con mayor número de incidencias.
- Total de incidencias.
- Tendencias mensuales de no conformidades y feedback.
- Estos datos ayudarán al Responsable de Calidad a identificar problemas recurrentes, evaluar mejoras y preparar auditorías.

## Entidades del Dominio para KPIs
Ya tenemos NonConformity y CustomerFeedback, pero necesitamos una entidad para QualityReport solo si deseamos persistir snapshots históricos. Si es en tiempo real, usaremos directamente queries al repositorio.

## Endpoint REST
Este endpoint permite obtener el dashboard de calidad desde un cliente HTTP.

```csharp
public static class EndpointsMapper
{
    public static IEndpointRouteBuilder MapQualityDashboardEndpoints(
        this IEndpointRouteBuilder builder)
    {
        builder.MapGet("{companyId}/dashboard/".CreateEndpoint("DashBoardEndpoints"), async (
            string companyId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? end,
            IGetQualityDashBoardInputPort inputPort) =>
        {
            var result = await inputPort.HandleAsync(companyId, from, end);
            return TypedResults.Ok(result);
        });

        return builder;

    }
}
```
### Reponse: AuditEventResponse
La respuesta es un objeto QualityDashboardResponse. Este objeto contiene
- Número de no conformidades abiertas.
- Número de no conformidades cerradas.
- Tiempo promedio de resolución de no conformidades.
- Total de feedbacks recibidos.
- Calificación promedio.
- Reportes de incidencia por EntityId.
- Total de reportes de incidencia.
- Tendencias mensuales de no conformidades y feedback.

```csharp
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
```

```csharp
public class MonthlyQualityKpi(
    string year, string month, int nonConformities, int feedbacks)
{
    public string Year => year;
    public string Month => month;
    public int NonConformities => nonConformities;
    public int Feedbacks => feedbacks;
}
```

## Repositorio: IGetQualityDashBoardRepository

```csharp
public interface IQueryableQualityDashboardRepository
{
    Task<int> GetNonConformitiesCountByStatus(string companyId, string status, DateTime? from, DateTime? end);
    Task<int> GetOpenNonConformitiesCount(string companyId, string closedStatus, DateTime? from, DateTime? end);
    Task<TimeSpan> GetAverageResolutionDays(string companyId, DateTime? from, DateTime? end);
    Task<int> GetTotalCustomerFeedbacks(string companyId, DateTime? from, DateTime? end);
    Task<double> GetAverageRatingOfCustomerFeedback(string companyId, DateTime? from, DateTime? end);
    Task<int> GetTotalIncidentReports(string companyId, DateTime? from, DateTime? end);
    Task<Dictionary<string, int>> GetIncidentReportsByEntityId(string companyId, DateTime? from, DateTime? end);
    Task<List<MonthlyQualityKpi>> GetMonthlyQualityKpis(string companyId, DateTime? from, DateTime? end);
}
```

### Implementación del Repositorio.
En la implementación del repositorio, se inyectan los contextos de datos necesarios, o en su defecto, las abstracciones de los repositorios de otros casos de uso.
```csharp
internal class QueryableQualityDashboardRepository(
    IQueryableCustomerFeedbackRepository getAllCustomerFeedbackRepository,
    IQueryableIncidentReportRepository getAllIncidentReportRepository,
    IQueryableNonConformityRepository getAllNonConformitiesRepository,
    IQueryableNonConformityDataContext nonConformityDataContext) : IQueryableQualityDashboardRepository
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
```


## Caso de uso: IGetQualityDashBoardInputPort

```csharp
public interface IGetQualityDashBoardInputPort
{
    Task<QualityDashboardResponse> HandleAsync(string companyId, DateTime? from, DateTime? end);
}
```

### Implementación del Caso de uso.

```csharp
internal class GetQualityDashBoardHandler(
    IQueryableQualityDashboardRepository repository) : IGetQualityDashBoardInputPort
{
    private const string NonConformityStatusClosed = "closed";

    public async Task<QualityDashboardResponse> HandleAsync(string companyId,
        DateTime? from, DateTime? end)
    {
        DateTime UtcFrom = from != null ? from.Value.Date
            : DateTime.UtcNow.Date.AddDays(-30);

        DateTime UtcEnd = end != null ? end.Value.Date.AddDays(1).AddTicks(-1)
            : DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

        int ClosedNonConformities =
            await repository.GetNonConformitiesCountByStatus(companyId, NonConformityStatusClosed, UtcFrom, UtcEnd);

        int OpenNonConformities =
            await repository.GetOpenNonConformitiesCount(companyId, NonConformityStatusClosed, UtcFrom, UtcEnd);

        TimeSpan AvarageNonConformityResolutionDays =
            await repository.GetAverageResolutionDays(companyId, UtcFrom, UtcEnd);

        int CustomerFeedbacks = await repository.GetTotalCustomerFeedbacks(companyId, UtcFrom, UtcEnd);

        double AvarageCustomerFeedback = await repository.GetAverageRatingOfCustomerFeedback(companyId, UtcFrom, UtcEnd);

        int IncidentReports = await repository.GetTotalIncidentReports(companyId, UtcFrom, UtcEnd);

        Dictionary<string, int> IncidentsPerOrder = await repository.GetIncidentReportsByEntityId(companyId, UtcFrom, UtcEnd);

        List<MonthlyQualityKpi> MonthlyQualityKpis = await repository.GetMonthlyQualityKpis(companyId, UtcFrom, UtcEnd);

        return new QualityDashboardResponse(
            OpenNonConformities,
            ClosedNonConformities,
            AvarageNonConformityResolutionDays,
            CustomerFeedbacks,
            AvarageCustomerFeedback,
            IncidentsPerOrder,
            IncidentReports,
            MonthlyQualityKpis
            );

    }
}
```

## ViewModel:  QualityDashboardViewModel
```csharp
public sealed class QualityDashboardViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public int OpenNonConformities { get; set; }

    public int ClosedNonConformities { get; set; }

    public double AverageResolutionDays { get; set; }

    public int TotalFeedbacks { get; set; }

    public double AverageRating { get; set; }

    public Dictionary<string, int> IncidentsPerOrder { get; set; } = new();

    public List<MonthlyQualityKpi> MonthlyKpis { get; set; } = new();

    public bool HasData { get; set; }

    public void Clear()
    {
        OpenNonConformities = 0;
        ClosedNonConformities = 0;
        AverageResolutionDays = 0;
        TotalFeedbacks = 0;
        AverageRating = 0;
        IncidentsPerOrder.Clear();
        MonthlyKpis.Clear();
        HasData = false;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }
}
```

# Dashboard en Blazor – Página .razor
La estructura básica de la página Blazor tendrá las siguientes secciones:
- Encabezado con estadísticas generales.
- Tabla con los incidentes por pedido.
- Gráficos o tablas con KPIs mensuales.
 
## Empezamos con la página .razor.
```razor
@page "/quality-dashboard"
@inject IGetQualityDashboardUseCase DashboardUseCase
@inject QualityDashboardPresenter DashboardPresenter
@inject QualityDashboardViewModel DashboardViewModel

@{
    // Set up dates (you can use current date or range, here we use last 30 days)
    DateTime fromDate = DateTime.Now.AddDays(-30);
    DateTime toDate = DateTime.Now;

    // Trigger dashboard loading when component is initialized
    Task.Run(() => DashboardUseCase.HandleAsync(new GetQualityDashboardRequest(fromDate, toDate), DashboardPresenter));
}

<div class="container mt-5">
    <h1 class="title is-3">Quality Dashboard</h1>

    @if (DashboardViewModel.HasData)
    {
        <!-- General Statistics -->
        <div class="columns is-multiline">
            <div class="column is-one-third">
                <div class="box">
                    <p class="title is-5">Open Non-Conformities</p>
                    <p class="subtitle is-4">@DashboardViewModel.OpenNonConformities</p>
                </div>
            </div>

            <div class="column is-one-third">
                <div class="box">
                    <p class="title is-5">Closed Non-Conformities</p>
                    <p class="subtitle is-4">@DashboardViewModel.ClosedNonConformities</p>
                </div>
            </div>

            <div class="column is-one-third">
                <div class="box">
                    <p class="title is-5">Average Resolution Time (Days)</p>
                    <p class="subtitle is-4">@DashboardViewModel.AverageResolutionDays</p>
                </div>
            </div>
        </div>

        <!-- Customer Feedback -->
        <div class="columns is-multiline">
            <div class="column is-half">
                <div class="box">
                    <p class="title is-5">Total Feedbacks</p>
                    <p class="subtitle is-4">@DashboardViewModel.TotalFeedbacks</p>
                </div>
            </div>

            <div class="column is-half">
                <div class="box">
                    <p class="title is-5">Average Rating</p>
                    <p class="subtitle is-4">@DashboardViewModel.AverageRating</p>
                </div>
            </div>
        </div>

        <!-- Incidents by Order -->
        <div class="box">
            <p class="title is-5">Incidents by Order</p>
            <table class="table is-fullwidth">
                <thead>
                    <tr>
                        <th>Order ID</th>
                        <th>Incidents</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var order in DashboardViewModel.IncidentsPerOrder)
                    {
                        <tr>
                            <td>@order.Key</td>
                            <td>@order.Value</td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>

        <!-- Monthly KPIs -->
        <div class="box">
            <p class="title is-5">Monthly KPIs</p>
            <table class="table is-fullwidth">
                <thead>
                    <tr>
                        <th>Month</th>
                        <th>Non-Conformities</th>
                        <th>Feedbacks</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var kpi in DashboardViewModel.MonthlyKpis)
                    {
                        <tr>
                            <td>@kpi.Month</td>
                            <td>@kpi.NonConformities</td>
                            <td>@kpi.Feedbacks</td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    }
    else
    {
        <div class="notification is-info">
            <p>No data available for the selected time period.</p>
        </div>
    }
</div>
```
### Explicación de la Página:
Encabezado General:

- Se muestra el número de Non-Conformities abiertas y cerradas, y el promedio de resolución en días.
- Se muestran las estadísticas de feedbacks y la calificación promedio.
- Incidentes por Pedido y total de incidentes.
- Una tabla muestra el número de incidentes relacionados con cada pedido.
- KPIs Mensuales:
- Una tabla muestra los KPIs mensuales, que incluyen el número de non-conformities y feedbacks por mes.

### CSS y Estilo con Bulma:
El uso de Bulma es sencillo, sin necesidad de añadir mucho CSS extra, solo usando las clases estándar de Bulma.

Las tablas son responsivas y la interfaz se adapta para ver las métricas de calidad de manera clara.


