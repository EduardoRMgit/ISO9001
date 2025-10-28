# Panel de auditoría

# Caso de uso: GetAuditEvents
El caso de uso GetAuditEvents es responsable de obtener un panel de auditoria para una entidad dentro de una compañía específica. Este planel incluye todos los eventos de auditoria registrados en el sistema:
- AuditLog
- CustomerFeedback
- IncidentReport
- NonConformity


## Endpoint REST
Este endpoint permite obtener el panel de auditoria desde un cliente HTTP.

```csharp
public static class EndpointsMapper
{
    public static IEndpointRouteBuilder MapAuditEventEndpoints(
        this IEndpointRouteBuilder builder)
    {
        builder.MapGet("{companyId}/".CreateEndpoint("AuditEventEndpoints"), async (
            string companyId,
            [FromQuery] string entityId,
            IGetAuditEventsInputPort inputPort) =>
        {
            var result = await inputPort.HandleAsync(entityId, companyId);
            return TypedResults.Ok(result);
        });

        return builder;

    }
}
```
### Reponse: AuditEventResponse
La respuesta es una colección de objetos AuditEventResponse. Cada objeto indica el tipo de evento en el campo EventType.

```csharp
{
    public class AuditEventResponse(string id, string entityId, DateTime timeStamp,
        string eventType, string description, string responsibleUser)
    {
        public string Id => id;
        public string EntityId => entityId;
        public DateTime TimeStamp => timeStamp;
        public string EventType => eventType;
        public string Description => description;
        public string ResponsibleUser => responsibleUser;
    }
}
```

## Repositorio: IGetAuditEventsRepository

```csharp
public interface IQueryableAuditEventRepository
{
    Task<IEnumerable<AuditEventResponse>> GetAuditEventsAsync(string entityId, string companyId);

}
```

### Implementación del Repositorio.
Se utiliza un patrón de proveedores de eventos de auditoría (IAuditEventProvider). Cada tipo de evento implementa su propio proveedor, y el repositorio central los unifica.
```csharp
internal class QueryableAuditEventRepository(IEnumerable<IAuditEventProvider> providers) : IQueryableAuditEventRepository
{
    public async Task<IEnumerable<AuditEventResponse>> GetAuditEventsAsync(string entityId, string companyId)
    {
        List<AuditEventResponse> AllAuditEvents = [];

        foreach (IAuditEventProvider provider in providers)
        {
            var AuditEvents = await provider.GetAuditEventsAsync(entityId, companyId);

            AllAuditEvents.AddRange(AuditEvents);
        }

        return AllAuditEvents;

    }
}
```

##### AuditLogEventProvider
```csharp
internal class AuditLogEventProvider(IQueryableAuditLogDataContext context) : IAuditEventProvider
{
    public string EventType => "AuditLog";

    public async Task<IEnumerable<AuditEventResponse>> GetAuditEventsAsync(string entityId, string companyId)
    {
        var AuditLogs = context.AuditLogs.Where
            (AuditLog => AuditLog.EntityId == entityId && 
            AuditLog.CompanyId == companyId)
            .OrderBy(AuditLog => AuditLog.LogId)
            .Select(AuditLog => new AuditEventResponse(
                AuditLog.LogId.ToString(), 
                AuditLog.EntityId,
                AuditLog.Timestamp, 
                EventType, 
                AuditLog.Details,
                AuditLog.PerformedBy));

        return await Task.FromResult(AuditLogs);
    }
}
```
##### CustomerFeedbackEventProvider
```csharp
internal class CustomerFeedbackEventProvider(IQueryableCustomerFeedbackDataContext context) : IAuditEventProvider
{
    public string EventType => "CustomerFeedback";

    public async Task<IEnumerable<AuditEventResponse>> GetAuditEventsAsync(string entityId, string companyId)
    {
        var CustomerFeedbacks = context.CustomerFeedbacks.Where
            (CustomerFeedback => CustomerFeedback.EntityId == entityId &&
            CustomerFeedback.CompanyId == companyId)
            .OrderBy(CustomerFeedback => CustomerFeedback.Id)
            .Select(CustomerFeedback => new AuditEventResponse(
                CustomerFeedback.Id.ToString(),
                CustomerFeedback.EntityId,
                CustomerFeedback.ReportedAt,
                EventType,
                CustomerFeedback.Comments,
                CustomerFeedback.CustomerId));

        return await Task.FromResult(CustomerFeedbacks);
    }
}
```

##### IncidentReportEventProvider
```csharp
internal class IncidentReportEventProvider(IQueryableIncidentReportDataContext context): IAuditEventProvider
{
    public string EventType => "IncidentReport";

    public async Task<IEnumerable<AuditEventResponse>> GetAuditEventsAsync(string entityId, string companyId)
    {
        var IncidentReports = context.IncidentReports.Where
            (IncidentReport => IncidentReport.EntityId == entityId &&
                IncidentReport.CompanyId == companyId)
            .OrderBy(IncidentReport => IncidentReport.Id)
            .Select(IncidentReport => new AuditEventResponse(
                IncidentReport.Id.ToString(),
                IncidentReport.EntityId,
                IncidentReport.ReportedAt,
                EventType,
                IncidentReport.Description,
                IncidentReport.UserId
                ));

        return await Task.FromResult(IncidentReports);
    }
}
```

##### NonConformityEventProvider
```csharp
internal class NonConformityEventProvider(IQueryableNonConformityDataContext context): IAuditEventProvider
{
    public string EventType => "NonConformity";

    public async Task<IEnumerable<AuditEventResponse>> GetAuditEventsAsync(string entityId, string companyId)
    {
        var Query = context.NonConformities
            .Where(NonConformity => NonConformity.EntityId == entityId && NonConformity.CompanyId == companyId)
            .OrderBy(NonConformity => NonConformity.ReportedAt);

        var NonConformities = await context.ToListAsync(Query);

        var Result = NonConformities.Select(NonConformity =>
        {
            var LastDetail = context.NonConformityDetails
            .Where(Detail => Detail.NonConformityId == NonConformity.Id)
            .OrderByDescending(Detail => Detail.CreatedAt)
            .FirstOrDefault();

            return new AuditEventResponse(
                NonConformity.Id.ToString(),
                NonConformity.EntityId,
                NonConformity.ReportedAt,
                EventType,
                LastDetail.Description,
                LastDetail.ReportedBy
                );
        });

        return await Task.FromResult(Result);
    }
}
```

## Caso de uso: IGetAuditEventsInputPort

```csharp
public interface IGetAuditEventsInputPort
{
    Task<IEnumerable<AuditEventResponse>> HandleAsync(string entityId, string companyId);
}
```

### Implementación del Caso de uso.

```csharp
internal class GetAuditEventsHandler(IQueryableAuditEventRepository repository) : IGetAuditEventsInputPort
{
    public async Task<IEnumerable<AuditEventResponse>> HandleAsync(string entityId, string companyId)
    {

        return await repository.GetAuditEventsAsync(entityId, companyId);
    }
}
```

## ViewModel de Blazor (Binding para la Vista)
Creamos una clase que actúa como Model en el patrón MVVM (la lógica del componente se separa aquí):

```csharp
public sealed class AuditLogPageModel
{
    public AuditLogViewModel ViewModel { get; } = new AuditLogViewModel();

    private readonly IGetAuditLogByOrderUseCase _useCase;
    private readonly AuditLogPresenter _presenter;

    public AuditLogPageModel(IGetAuditLogByOrderUseCase useCase)
    {
        _presenter = new AuditLogPresenter(ViewModel);
        _useCase = useCase;
    }

    public async Task LoadAsync(Guid orderId)
    {
        GetAuditLogByOrderRequest request = new GetAuditLogByOrderRequest(orderId);
        await _useCase.HandleAsync(request, _presenter);
    }
}
```

## Componente Blazor – AuditLog.razor
Este es el componente de interfaz que usará Bulma para mostrar los eventos:

```razor
@page "/orders/{OrderId:guid}/audit"
@inject IGetAuditLogByOrderUseCase UseCase

@code {
    [Parameter]
    public Guid OrderId { get; set; }

    private AuditLogPageModel model = default!;
    private bool isLoaded = false;

    protected override async Task OnInitializedAsync()
    {
        model = new AuditLogPageModel(UseCase);
        await model.LoadAsync(OrderId);
        isLoaded = true;
    }
}
@if (!isLoaded)
{
    <p class="has-text-centered">Loading audit log...</p>
}
else
{
    <div class="box">
        <h1 class="title is-4">Audit Trail</h1>

        <table class="table is-striped is-fullwidth">
            <thead>
                <tr>
                    <th>Date</th>
                    <th>Event</th>
                    <th>Description</th>
                    <th>Responsible</th>
                </tr>
            </thead>
            <tbody>
                @foreach (AuditLogItemViewModel item in model.ViewModel.Events)
                {
                    <tr>
                        <td>@item.Timestamp</td>
                        <td>@item.EventType</td>
                        <td>@item.Description</td>
                        <td>@item.ResponsibleUser</td>
                    </tr>
                }
            </tbody>
        </table>
    </div>
}
```
