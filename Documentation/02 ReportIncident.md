# Entidad: IncidentReport
Permite cumplir con el requisito de gestión de no conformidades y acciones correctivas.
La entidad IncidentReport almacenará los reportes de incidencias.

CompanyId: Identificador de la empresa propietaria del reporte de incidencia.

EntityId: Identificador de la entidad afectada.

ReportedAt: La fecha y hora del reporte.

UserId: El usuario que ha realizado la acción (ID del cliente o del administrador).

Description: Descripción del reporte de incidencia.

AffectedProcess: Proceso involucrado (Order, Payment, etc).

Severity: Nivel de importancia (Low, Medium, High).

Data: Datos en formato JSON relacionados al reporte.

```csharp
public class IncidentReport
{
    public string CompanyId { get; set; }
    public string EntityId { get; set; }
    public DateTime ReportedAt { get; set; }
    public string UserId { get; set; }
    public string Description { get; set; }
    public string AffectedProcess { get; set; }
    public string Severity { get; set; }
    public string Data { get; set; }
}
```

## Repository: Interfaces
En esta sección se definen las interfaces que se utilizarán en los repositorios de los casos de uso de la entidad IncidentReport. Siguiendo el patrón CQRS, se separan las operaciones de lectura y escritura en dos interfaces diferentes.

### ICommandIncidentReportRepository
La interfaz ICommandIncidentReportRepository se encarga únicamente de agregar y guardar registros IncidentReport.

```csharp
public interface ICommandIncidentReportRepository
{
    Task RegisterIncidentReportAsync(IncidentReportDto incidentReport);
    Task SaveChangesAsync();
}
```

### IQueryableIncidentReportRepository
La interfaz IQueryableIncidentReportRepository está dedicada a las operaciones de consulta.

```csharp
public interface IQueryableIncidentReportRepository
{
    Task<IEnumerable<IncidentReportResponse>> GetAllIncidentReportsAsync(string id, DateTime? from, DateTime? end);

    Task<IncidentReportResponse> GetIncidentReportByIdAsync(string companyId, int id);

    Task<bool> IncidentReportExists(string companyId, int id);
}
```

## Implementación de los repositorios.

### CommandIncidentReportRepository
```csharp
internal class CommandIncidentReportRepository(IWritableIncidentReportDataContext
    dataContext) : ICommandIncidentReportRepository
{
    public async Task RegisterIncidentReportAsync(IncidentReportDto incidentReportDto)
    {
        var NewIncidentReport = new Entities.IncidentReport
        {
            CompanyId = incidentReportDto.CompanyId,
            EntityId = incidentReportDto.EntityId,
            ReportedAt = incidentReportDto.ReportedAt,
            UserId = incidentReportDto.UserId,
            Description = incidentReportDto.Description,
            AffectedProcess = incidentReportDto.AffectedProcess,
            Severity = incidentReportDto.Severity,
            Data = incidentReportDto.Data
        };

        await dataContext.AddAsync(NewIncidentReport);
    }

    public Task SaveChangesAsync() => dataContext.SaveChangesAsync();
}
```

### QueryableIncidentReportRepository
```csharp
internal class QueryableIncidentReportRepository
    (IQueryableIncidentReportDataContext dataContext) : IQueryableIncidentReportRepository
{
    public async Task<IEnumerable<IncidentReportResponse>> GetAllIncidentReportsAsync(string id, DateTime? from, DateTime? end)
    {
        var Query = dataContext.IncidentReports
            .Where(IncidentReport =>
                IncidentReport.CompanyId == id &&
                IncidentReport.ReportedAt >= from &&
                IncidentReport.ReportedAt <= end)
            .OrderBy(IncidentReport => IncidentReport.ReportedAt);

        var IncidentReports = await dataContext.ToListAsync(Query);

        return IncidentReports.Select(
            IncidentReport => new IncidentReportResponse(
                IncidentReport.EntityId,
                IncidentReport.ReportedAt,
                IncidentReport.UserId,
                IncidentReport.Description,
                IncidentReport.AffectedProcess,
                IncidentReport.Severity,
                IncidentReport.Data
                ));
    }

    public Task<IncidentReportResponse> GetIncidentReportByIdAsync(string companyId, int id)
    {
        var IncidentReport = dataContext.IncidentReports
            .FirstOrDefault(IncidentReport => IncidentReport.CompanyId == companyId &&
            IncidentReport.Id == id);

        return Task.FromResult(new IncidentReportResponse(
            IncidentReport.EntityId,
            IncidentReport.ReportedAt,
            IncidentReport.UserId,
            IncidentReport.Description,
            IncidentReport.AffectedProcess,
            IncidentReport.Severity,
            IncidentReport.Data));
    }

    public Task<bool> IncidentReportExists(string companyId, int id)
    {
        var IncidentReport = dataContext.IncidentReports
            .FirstOrDefault(IncidentReport => IncidentReport.CompanyId == companyId &&
            IncidentReport.Id == id);

        return Task.FromResult(IncidentReport != null);
    }
}
```



## DataContext: Interfaces

En esta sección se definirán las interfaces que se utilizarán en los repositorios de los diferentes casos de usos de la entidad IncidentReport. Siguiendo el patrón CQRS, se separaron las operaciones de lectura y escritura en dos interfaces diferentes.

### IWritableIncidentReportDataContext

La interfaz IWritableIncidentReportDataContext se encarga exclusivamente de agregar y guardar los registros

```csharp
public interface IWritableIncidentReportDataContext
{
    Task AddAsync(Entities.IncidentReport incidentReport);
    Task SaveChangesAsync();
}
```

### IQueryableIncidentReportDataContext

La interfaz IQueryableIncidentReportDataContext está dedicada a las operaciones de lectura, con un IQueryable que expone los reportes de incidencia y un método para obtener los resultados.

```csharp
public interface IQueryableIncidentReportDataContext
{
    IQueryable<IncidentReportReadModel> IncidentReports { get; }
    Task<IEnumerable<IncidentReportReadModel>> ToListAsync(
        IQueryable<IncidentReportReadModel> queryable);
}
```

## Implementación de los DataContext
Puedes implementar ambos contextos de datos utilizando un sistema de base de datos o almacenamiento de archivos. A continuación se presenta un ejemplo simple de una implementación en memoria para la persistencia de datos.

### InMemoryIncidentReportStore

```csharp
internal class InMemoryIncidentReportStore
{
    public List<Entities.IncidentReport> IncidentReports { get; } = new();
    public int IncidentReportCurrentId { get; set; }
}
```


### InMemoryWritableIncidentReportDataContext

```csharp
internal class InMemoryWritableIncidentReportDataContext(
    InMemoryIncidentReportStore dataContext) : IWritableIncidentReportDataContext
{
    public Task AddAsync(Repositories.IncidentReportRepositories.Entities.IncidentReport incidentReport)
    {
        var Record = new DataContexts.Entities.IncidentReport
        {
            Id = ++dataContext.IncidentReportCurrentId,
            CompanyId = incidentReport.CompanyId,
            EntityId = incidentReport.EntityId,
            ReportedAt = incidentReport.ReportedAt,
            CreatedAt = DateTime.UtcNow,
            UserId = incidentReport.UserId,
            Description = incidentReport.Description,
            AffectedProcess = incidentReport.AffectedProcess,
            Severity = incidentReport.Severity,
            Data = incidentReport.Data
        };

        dataContext.IncidentReports.Add(Record);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        return Task.CompletedTask;
    }
}
```

### InMemoryQueryableIncidentReportDataContext

```csharp
internal class InMemoryQueryableIncidentReportDataContext(
    InMemoryIncidentReportStore dataContext): IQueryableIncidentReportDataContext
{
    public IQueryable<IncidentReportReadModel> IncidentReports =>
        dataContext.IncidentReports
        .Select(IncidentReport => new IncidentReportReadModel
        {
            Id = IncidentReport.Id,
            CompanyId = IncidentReport.CompanyId,
            EntityId = IncidentReport.EntityId,
            ReportedAt = IncidentReport.ReportedAt,
            CreatedAt = IncidentReport.CreatedAt,
            UserId = IncidentReport.UserId,
            Description = IncidentReport.Description,
            AffectedProcess = IncidentReport.AffectedProcess,
            Severity = IncidentReport.Severity,
            Data = IncidentReport.Data
        }).AsQueryable();

    public async Task<IEnumerable<IncidentReportReadModel>> ToListAsync(IQueryable<IncidentReportReadModel> queryable)
        => await Task.FromResult(queryable.ToList());

}
```
# Caso de uso: RegisterIncidentReport
El caso de uso RegisterIncidentReport es responsable de registrar reportes de incidencia en el sistema.

## Parametros de Entrada.
- IncidentReportRequest (obligatorio).

## Endpoint REST
Este endpoint permite registrar reportes de incidencia desde un cliente HTTP.

```csharp
public static class EndpointsMapper
{
    public static IEndpointRouteBuilder MapIncidentReportEndpoints(
        this IEndpointRouteBuilder builder)
    {
            builder.MapPost("".CreateEndpoint("IncidentReportEndpoints"),
                async (IncidentReportRequest incidentReport, IRegisterIncidentReportInputPort inputport) =>
                {
                    await inputport.HandleAsync(new IncidentReportDto(
                        incidentReport.CompanyId,
                        incidentReport.EntityId,
                        incidentReport.ReportedAt,
                        incidentReport.UserId,
                        incidentReport.Description,
                        incidentReport.AffectedProcess,
                        incidentReport.Severity,
                        incidentReport.Data)
                        );
                    return TypedResults.Created();
                });
    }
}
```
### DTO y Request

```csharp
public class IncidentReportDto(string companyId, string entityId, DateTime reportedAt,
    string userId, string description, string affectedProcess, string severity, string data)
{
    public string CompanyId => companyId;
    public string EntityId => entityId;
    public DateTime ReportedAt => reportedAt;
    public string UserId => userId;
    public string Description => description;
    public string AffectedProcess => affectedProcess;
    public string Severity => severity;
    public string Data => data;
}
```

```csharp
public class IncidentReportRequest
{
    public string CompanyId { get; set; }
    public string EntityId { get; set; }
    public DateTime ReportedAt { get; set; }
    public string UserId { get; set; }
    public string Description { get; set; }
    public string AffectedProcess { get; set; }
    public string Severity { get; set; }
    public string Data { get; set; }
}
```
## Caso de uso: IRegisterIncidentReportInputPort

```csharp
public interface IRegisterIncidentReportInputPort
{
    Task HandleAsync(IncidentReportDto incidentReportDto);
}
```

### Implementación del Caso de uso.

```csharp
internal class RegisterIncidentReportHandler(IRegisterIncidentReportRepository
    repository) : IRegisterIncidentReportInputPort
{
    public async Task HandleAsync(IncidentReportDto incidentReportDto)
    {
        await repository.RegisterIncidentReportAsync(incidentReportDto);
        await repository.SaveChangesAsync();
    }
}
```

# Integración en Blazor WebAssembly (UI)

Después de realizar un pedido, en el componente Blazor, podemos mostrar el mensaje de éxito, y si se ha guardado el log correctamente.

```razor
@page "/place-order"
@inject PlaceOrderVM ViewModel

<h3>Place Order</h3>

<!-- Formulario de pedido aquí -->

<button class="button is-primary" @onclick="PlaceOrder">Place Order</button>

@if (ViewModel.Result != null)
{
    <div class="notification is-success">
        <p>Order placed successfully!</p>
        <p>Order ID: @ViewModel.Result.OrderId</p>
    </div>
}

@code {
    private async Task PlaceOrder()
    {
        await ViewModel.PlaceOrderAsync();
    }
}
```

# Caso de uso: GetAllIncidentReports
El caso de uso GetAllIncidentReports es responsable de obtener todos los reportes de incidencia del sistema para una compañía específica dentro de un rango de fechas.

## Parametros de entrada.
- companyId (obligatorio): Identificador de la empresa cuyos registros se desean consultar.
- from (opcional): Fecha de inicio del rango. Si no se especifica, se toma como valor predeterminado 30 días antes del día actual.
- end (opcional): Fecha de fin del rango. Si no se especifica, se toma como valor predeterminado el final del día actual.


## Endpoint REST
Este endpoint permite obtener los reportes de incidencia desde un cliente HTTP.

```csharp
public static class EndpointsMapper
{
    public static IEndpointRouteBuilder MapIncidentReportEndpoints(
        this IEndpointRouteBuilder builder)
    {
            builder.MapGet("{companyId}/".CreateEndpoint("IncidentReportEndpoints"), async (
                string companyId,
                [FromQuery] DateTime? from,
                [FromQuery] DateTime? end,
                IGetAllIncidentReportsInputPort inputPort) =>
            {
                var result = await inputPort.HandleAsync(companyId, from, end);
                return TypedResults.Ok(result);

            });
    }
}
```
### Reponse: IncidentReportResponse

```csharp
public class IncidentReportResponse(string entityId, DateTime reportedAt, string userId,
    string description, string affectedProcess, string severity, string data)
{
    public string EntityId => entityId;
    public DateTime ReportedAt => reportedAt;
    public string UserId => userId;
    public string Description => description;
    public string AffectedProcess => affectedProcess;
    public string Severity => severity;
    public string Data => data;
}
```
## Caso de uso: IGetAllIncidentReportsInputPort

```csharp
public interface IGetAllIncidentReportsInputPort
{
    Task<IEnumerable<IncidentReportResponse>> HandleAsync(string id, DateTime? from, DateTime? end);
}
```

### Implementación del Caso de uso.

```csharp
internal class GetAllIncidentReportsHandler(IGetAllIncidentReportsRepository
    repository): IGetAllIncidentReportsInputPort
{
    public async Task<IEnumerable<IncidentReportResponse>> HandleAsync(string id, DateTime? from, DateTime? end)
    {
        DateTime UtcFrom = from != null ? from.Value.Date
            : DateTime.UtcNow.Date.AddDays(-30);

        DateTime UtcEnd = end != null ? end.Value.Date.AddDays(1).AddTicks(-1)
            : DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

        return await repository.GetAllIncidentReportsAsync(id, UtcFrom, UtcEnd);
    }
}
```

# Integración en Blazor WebAssembly (UI)

Después de realizar un pedido, en el componente Blazor, podemos mostrar el mensaje de éxito, y si se ha guardado el log correctamente.

```razor
@page "/place-order"
@inject PlaceOrderVM ViewModel

<h3>Place Order</h3>

<!-- Formulario de pedido aquí -->

<button class="button is-primary" @onclick="PlaceOrder">Place Order</button>

@if (ViewModel.Result != null)
{
    <div class="notification is-success">
        <p>Order placed successfully!</p>
        <p>Order ID: @ViewModel.Result.OrderId</p>
    </div>
}

@code {
    private async Task PlaceOrder()
    {
        await ViewModel.PlaceOrderAsync();
    }
}
```

# Caso de uso: GetIncidentReportById
El caso de uso GetIncidentReportById es responsable el reporte de incidencia a partir de un id.
## Parametros de entrada.
- companyId (obligatorio): Identificador de la empresa cuyos registros se desean consultar.
- id (obligatorio): Id del reporte de incidencia.

## Endpoint REST
Este endpoint permite obtener el reporte de incidencia a partir de un id desde un cliente HTTP.

```csharp
public static class EndpointsMapper
{
    public static IEndpointRouteBuilder MapIncidentReportEndpoints(
        this IEndpointRouteBuilder builder)
    {
            builder.MapGet(("{companyId}/" + "Id" + "/{id}").CreateEndpoint("IncidentReportEndpoints"), async (
                string companyId,
                int id,
                IGetIncidentReportByIdInputPort inputPort) =>
            {
                var Result = await inputPort.HandleAsync(companyId, id);
                return TypedResults.Ok(Result);
            }
            );
    }
}
```
### Reponse: IncidentReportResponse

```csharp
public class IncidentReportResponse(string entityId, DateTime reportedAt, string userId,
    string description, string affectedProcess, string severity, string data)
{
    public string EntityId => entityId;
    public DateTime ReportedAt => reportedAt;
    public string UserId => userId;
    public string Description => description;
    public string AffectedProcess => affectedProcess;
    public string Severity => severity;
    public string Data => data;
}
```
## Caso de uso: IGetIncidentReportByIdInputPort

```csharp
public interface IGetIncidentReportByIdInputPort
{
    Task<IncidentReportResponse> HandleAsync(string companyId, int id);
}
```

### Implementación del Caso de uso.

```csharp
internal class GetIncidentReportByIdHandler(
    IQueryableIncidentReportRepository repository) : IGetIncidentReportByIdInputPort
{
    public async Task<IncidentReportResponse> HandleAsync(string companyId, int id)
    {
        var IncidentReportExists = await repository.IncidentReportExists(companyId, id);

        if (!IncidentReportExists)
        {
            throw new KeyNotFoundException($"IncidentReport with Id '{id}' doesn't exist in the company: '{companyId}'");
        }
        else
        {
            return await repository.GetIncidentReportByIdAsync(companyId, id);
        }

    }
}
```

# Integración en Blazor WebAssembly (UI)

Después de realizar un pedido, en el componente Blazor, podemos mostrar el mensaje de éxito, y si se ha guardado el log correctamente.

```razor
@page "/place-order"
@inject PlaceOrderVM ViewModel

<h3>Place Order</h3>

<!-- Formulario de pedido aquí -->

<button class="button is-primary" @onclick="PlaceOrder">Place Order</button>

@if (ViewModel.Result != null)
{
    <div class="notification is-success">
        <p>Order placed successfully!</p>
        <p>Order ID: @ViewModel.Result.OrderId</p>
    </div>
}

@code {
    private async Task PlaceOrder()
    {
        await ViewModel.PlaceOrderAsync();
    }
}
```

