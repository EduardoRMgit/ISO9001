# Entidad: AuditLog
La entidad AuditLog almacenará los registros de auditoría de los eventos importantes (por ejemplo, realizar un pedido). Este registro debería incluir:

EntityId: Identificador de la entidad afectada.

CompanyId: Identificador de la empresa propietaria del evento.

Action: Tipo de acción realizada

PerformedBy: El usuario que ha realizado la acción (ID del cliente o del administrador).

Timestamp: La fecha y hora del evento.

Details: Información adicional del evento.

Data: Datos en formato JSON relacionados con la acción.

```csharp
public class AuditLog
{
    public string EntityId { get; set; }
    public string CompanyId { get; set; }
    public string Action { get; set; }
    public string PerformedBy { get; set; }
    public DateTime Timestamp { get; set; }
    public string Details { get; set; }
    public string Data { get; set; }
}
```
## Repository: Interfaces
En esta sección se definen las interfaces que se utilizarán en los repositorios de los casos de uso de la entidad AuditLog. Siguiendo el patrón CQRS, se separan las operaciones de lectura y escritura en dos interfaces diferentes.

### ICommandAuditLogRepository
La interfaz ICommandAuditLogRepository se encarga únicamente de agregar y guardar registros AuditLog.

```csharp
public interface ICommandAuditLogRepository
{
    Task RegisterAuditLogAsync(AuditLogDto auditLog);
    Task SaveChangesAsync();
}
```

### IQueryableAuditLogRepository
La interfaz IQueryableAuditLogRepository está dedicada a las operaciones de consulta.

```csharp
public interface IQueryableAuditLogRepository
{
    Task<IEnumerable<AuditLogResponse>> GetAllAuditLogsOrderedByIdAscendingAsync(string id, DateTime? from, DateTime? end);

    Task<AuditLogResponse> GetAuditLogByIdAsync(string companyId, int id);

    Task<bool> AuditLogExitsByIdAsync(string companyId, int id);

    Task<IEnumerable<AuditLogResponse>> GetAuditLogsByActionAsync(string id, string action, DateTime? from, DateTime? end);

    Task<IEnumerable<AuditLogResponse>> GetAuditLogsByEntityIdAsync(string id, string entityId, DateTime? from, DateTime? end);
}
```

## Implementación de los repositorios.

### CommandAuditLogRepository
```csharp
internal class CommandAuditLogRepository(
    IWritableAuditLogDataContext dataContext) : ICommandAuditLogRepository
{
    public async Task RegisterAuditLogAsync(AuditLogDto auditLogDto)
    {

        var NewAuditiLog = new Entities.AuditLog
        {
            EntityId = auditLogDto.EntityId,
            CompanyId = auditLogDto.CompanyId,
            Action = auditLogDto.Action,
            PerformedBy = auditLogDto.PerformedBy,
            Timestamp = auditLogDto.Timestamp,
            Details = auditLogDto.Details,
            Data = auditLogDto.Data,
        };

        await dataContext.AddAsync(NewAuditiLog);

    }

    public async Task SaveChangesAsync() => await dataContext.SaveChangesAsync();

}
```

### QueryableAuditLogRepository
```csharp
internal class QueryableAuditLogRepository(IQueryableAuditLogDataContext dataContext) : IQueryableAuditLogRepository
{
    public async Task<IEnumerable<AuditLogResponse>> GetAuditLogsByEntityIdAsync(string id, string entityId,
    DateTime? from, DateTime? end)
    {
        IQueryable<AuditLogReadModel> Query = dataContext.AuditLogs
            .Where(AuditLog => AuditLog.CompanyId == id &&
                        AuditLog.EntityId == entityId &&
                        AuditLog.Timestamp >= from &&
                        AuditLog.Timestamp <= end);

        var AuditLogs = await dataContext.ToListAsync(Query);

        return AuditLogs.Select(AuditLog => new AuditLogResponse(
            AuditLog.LogId,
            AuditLog.EntityId,
            AuditLog.Action,
            AuditLog.PerformedBy,
            AuditLog.Timestamp,
            AuditLog.CreatedAt,
            AuditLog.Details));
    }

    public async Task<IEnumerable<AuditLogResponse>> GetAuditLogsByActionAsync(string id, string action,
    DateTime? from, DateTime? end)
    {
        IQueryable<AuditLogReadModel> Query = dataContext.AuditLogs
            .Where(AuditLog => AuditLog.CompanyId == id &&
                            AuditLog.Action == action &&
                            AuditLog.Timestamp >= from &&
                            AuditLog.Timestamp <= end);

        var AuditLogs = await dataContext.ToListAsync(Query);

        return AuditLogs.Select(AuditLog => new AuditLogResponse(
            AuditLog.LogId,
            AuditLog.EntityId,
            AuditLog.Action,
            AuditLog.PerformedBy,
            AuditLog.Timestamp,
            AuditLog.CreatedAt,
            AuditLog.Details));
    }

    public Task<bool> AuditLogExitsByIdAsync(string companyId, int id)
    {
        var AuditLog = dataContext.AuditLogs.FirstOrDefault
            (AuditLog => AuditLog.CompanyId == companyId &&
                AuditLog.LogId == id);

        return Task.FromResult(AuditLog != null);
    }

    public Task<AuditLogResponse> GetAuditLogByIdAsync(string companyId, int id)
    {
        var AuditLog = dataContext
            .AuditLogs.FirstOrDefault(
            AuditLog => AuditLog.CompanyId == companyId &&
                AuditLog.LogId == id);

        return Task.FromResult(new AuditLogResponse(
            AuditLog.LogId,
            AuditLog.EntityId,
            AuditLog.Action,
            AuditLog.PerformedBy,
            AuditLog.Timestamp,
            AuditLog.CreatedAt,
            AuditLog.Details));
    }

    public async Task<IEnumerable<AuditLogResponse>> GetAllAuditLogsOrderedByIdAscendingAsync(
    string id, DateTime? from, DateTime? end)
    {
        IQueryable<AuditLogReadModel> Query = dataContext.AuditLogs
            .Where(AuditLog => AuditLog.CompanyId == id &&
                            AuditLog.Timestamp >= from &&
                            AuditLog.Timestamp <= end)
            .OrderBy(AuditLog => AuditLog.LogId);

        var AuditLogs = await dataContext.ToListAsync(Query);

        return AuditLogs.Select(AuditLog => new AuditLogResponse(
            AuditLog.LogId,
            AuditLog.EntityId,
            AuditLog.Action,
            AuditLog.PerformedBy,
            AuditLog.Timestamp,
            AuditLog.CreatedAt,
            AuditLog.Details));
    }
}
```


## DataContext: Interfaces

En esta sección se definirán las interfaces que se utilizarán en los datacontext de los diferentes casos de uso de la entidad AuditLog. Siguiendo el patrón CQRS, se separaron las operaciones de lectura y escritura en dos interfaces diferentes.

### IWritableAuditLogDataContext

La interfaz IWritableAuditLogDataContext se encarga exclusivamente de agregar y guardar los registros

```csharp
public interface IWritableAuditLogDataContext
{
    Task AddAsync(Entities.AuditLog auditLog);
    Task SaveChangesAsync();
}
```

### IQueryableAuditLogDataContext

La interfaz IQueryableAuditLogDataContext está dedicada a las operaciones de lectura, con un IQueryable que expone los registros  de auditoria y un método para obtener los resultados.

```csharp
public interface IQueryableAuditLogDataContext
{
    IQueryable<AuditLogReadModel> AuditLogs { get; }
    Task<IEnumerable<AuditLogReadModel>> ToListAsync(
        IQueryable<AuditLogReadModel> queryable);
}
```

## Implementación de los DataContext
Puedes implementar ambos contextos de datos utilizando un sistema de base de datos o almacenamiento de archivos. A continuación se presenta un ejemplo simple de una implementación en memoria para la persistencia de datos.

### InMemoryAuditLogStore

```csharp
internal class InMemoryAuditLogStore
{
    public List<Entities.AuditLog> AuditLogs { get; } = new();
    public int CurrentId { get; set; }
}
```


### InMemoryWritableAuditLogDataContext

```csharp
internal class InMemoryWritableAuditLogDataContext(
    InMemoryAuditLogStore dataContext) : IWritableAuditLogDataContext
{
    public Task AddAsync(Repositories.AuditLogRepositories.Entities.AuditLog auditLog)
    {
        var Record = new DataContexts.Entities.AuditLog
        {
            Id = ++dataContext.CurrentId,
            CreatedAt = DateTime.UtcNow,
            EntityId = auditLog.EntityId,
            CompanyId = auditLog.CompanyId,
            Action = auditLog.Action,
            PerformedBy = auditLog.PerformedBy,
            Timestamp = auditLog.Timestamp,
            Details = auditLog.Details,
            Data = auditLog.Data
        };

        dataContext.AuditLogs.Add(Record);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        return Task.CompletedTask;
    }
}
```

### InMemoryQueryableAuditLogDataContext

```csharp
internal class InMemoryQueryableAuditLogDataContext(
    InMemoryAuditLogStore dataContext) : IQueryableAuditLogDataContext
{
    public IQueryable<AuditLogReadModel> AuditLogs =>
        dataContext.AuditLogs
            .Select(AuditLog => new AuditLogReadModel
            {
                LogId = AuditLog.Id,
                EntityId = AuditLog.EntityId,
                CompanyId = AuditLog.CompanyId,
                Action = AuditLog.Action,
                PerformedBy = AuditLog.PerformedBy,
                Timestamp = AuditLog.Timestamp,
                CreatedAt = AuditLog.CreatedAt,
                Details = AuditLog.Details
            }).AsQueryable();

    public async Task<IEnumerable<AuditLogReadModel>> ToListAsync(IQueryable<AuditLogReadModel> queryable)
        => await Task.FromResult(queryable.ToList());

}
```
# Caso de uso: RegisterAuditLog
El caso de uso RegisterAuditLog es responsable de registrar AuditLogs en el sistema.

## Parametros de Entrada.
- AuditLogRequest (obligatorio).
## Endpoint REST
Este endpoint permite registrar logs desde un cliente HTTP.

```csharp
public static class EndpointsMapper
{
    public static IEndpointRouteBuilder MapAuditLogEndpoints(
        this IEndpointRouteBuilder builder)
    {
        builder.MapPost("".CreateEndpoint("AuditLogEndpoints"),
            async (AuditLogRequest auditLog, IRegisterAuditLogInputPort inputPort) =>
            {
                await inputPort.HandleAsync(new AuditLogDto(
                    auditLog.EntityId,
                    auditLog.CompanyId,
                    auditLog.Action,
                    auditLog.PerformedBy,
                    auditLog.Timestamp,
                    auditLog.Details,
                    auditLog.Data)
                    );
                return TypedResults.Created();
            });
    }
}
```
### DTO y Request

```csharp
public class AuditLogDto(string entityId, string companyId, string action,
    string performedBy, DateTime timestamp, string details, string data)
{
    public string EntityId => entityId;
    public string CompanyId => companyId;
    public string Action => action;
    public string PerformedBy => performedBy;
    public DateTime Timestamp => timestamp;
    public string Details => details;
    public string Data => data;
}
```

```csharp
public class AuditLogRequest
{
    public string EntityId { get; set; }
    public string CompanyId { get; set; }
    public string Action { get; set; }
    public string PerformedBy { get; set; }
    public DateTime Timestamp { get; set; }
    public string Details { get; set; }
    public string Data { get; set; }
}
```
## Caso de uso: IRegisterAuditLogInputPort

```csharp
public interface IRegisterAuditLogInputPort
{
    Task HandleAsync(AuditLogDto auditLog);
}
```

### Implementación del Caso de uso.

```csharp
internal class RegisterAuditLogHandler(IRegisterAuditLogRepository repository) : IRegisterAuditLogInputPort
{
    public async Task HandleAsync(AuditLogDto auditLog)
    {
        await repository.RegisterAuditLogAsync(auditLog);
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

# Caso de uso: GetAllAuditLogs
El caso de uso GetAllAuditLogs es responsable de obtener todos los registros de auditoría (AuditLogs) del sistema para una compañía específica ordenados por Id.

## Parametros de entrada.
- companyId (obligatorio): Identificador de la empresa cuyos registros se desean consultar.
- from (opcional): Fecha de inicio del rango. Si no se especifica, se toma como valor predeterminado 30 días antes del día actual.
- end (opcional): Fecha de fin del rango. Si no se especifica, se toma como valor predeterminado el final del día actual.


## Endpoint REST
Este endpoint permite obtener los logs desde un cliente HTTP.

```csharp
public static class EndpointsMapper
{
    public static IEndpointRouteBuilder MapAuditLogEndpoints(
        this IEndpointRouteBuilder builder)
    {
            builder.MapGet("{companyId}/".CreateEndpoint("AuditLogEndpoints"), async (
            string companyId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? end,
            IGetAllAuditLogsInputPort inputPort) =>
            {
                var result = await inputPort.HandleAsync(companyId, from, end);
                return TypedResults.Ok(result);

            });
    }
}
```
### Reponse: AuditLogResponse

```csharp
public class AuditLogResponse(int logId, string entityId, string action,
    string performedBy, DateTime timeStamp, DateTime createdAt, string details)
{
    public int LogId => logId;
    public string EntityId => entityId;
    public string Action => action;
    public string PerformedBy => performedBy;
    public DateTime TimeStamp => timeStamp;
    public DateTime CreatedAt => createdAt;
    public string Details => details;
}
```
## Caso de uso: IGetAllAuditLogsInputPort

```csharp
public interface IGetAllAuditLogsInputPort
{
    Task<IEnumerable<AuditLogResponse>> HandleAsync(string id, DateTime? from, DateTime? end);
}
```

### Implementación del Caso de uso.

```csharp
internal class GetAllAuditLogsHandler(IGetAllAuditLogsRepository repository) : IGetAllAuditLogsInputPort
{
    public async Task<IEnumerable<AuditLogResponse>> HandleAsync(string id, DateTime? from, DateTime? end)
    {
        DateTime UtcFrom = from != null ? from.Value.Date
            : DateTime.UtcNow.Date.AddDays(-30);

        DateTime UtcEnd = end != null ? end.Value.Date.AddDays(1).AddTicks(-1)
            : DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

        return await repository.GetAllAuditLogsOrderedByIdAscendingAsync(id, UtcFrom, UtcEnd);
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

# Caso de uso: GetAuditLogsByAction
El caso de uso GetAuditLogsByAction es responsable de obtener todos los AuditLogs del sistema que coincidan con una acción específica (action) para una compañía dada (companyId).

## Parametros de entrada.
- companyId (obligatorio): Identificador de la empresa cuyos registros se desean consultar.
- action (obligatorio): Acción especifica que se desea filtrar.
- from (opcional): Fecha de inicio del rango. Si no se especifica, se toma como valor predeterminado 30 días antes del día actual.
- end (opcional): Fecha de fin del rango. Si no se especifica, se toma como valor predeterminado el final del día actual.


## Endpoint REST
Este endpoint permite obtener los logs desde un cliente HTTP.

```csharp
public static class EndpointsMapper
{
    public static IEndpointRouteBuilder MapAuditLogEndpoints(
        this IEndpointRouteBuilder builder)
    {
            builder.MapGet(("{companyId}/" + "Action" + "/{action}").CreateEndpoint("AuditLogEndpoints"), async (
            string companyId,
            string action,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? end,
            IGetAuditLogsByActionInputPort inputPort) =>
            {
                var result = await inputPort.HandleAsync(companyId, action, from, end);
                return TypedResults.Ok(result);

            });
    }
}
```
### Reponse: AuditLogResponse

```csharp
public class AuditLogResponse(int logId, string entityId, string action,
    string performedBy, DateTime timeStamp, DateTime createdAt, string details)
{
    public int LogId => logId;
    public string EntityId => entityId;
    public string Action => action;
    public string PerformedBy => performedBy;
    public DateTime TimeStamp => timeStamp;
    public DateTime CreatedAt => createdAt;
    public string Details => details;
}
```
## Caso de uso: IGetAuditLogsByActionInputPort

```csharp
public interface IGetAuditLogsByActionInputPort
{
    Task<IEnumerable<AuditLogResponse>> HandleAsync(string id, string action, DateTime? from, DateTime? end);
}
```

### Implementación del Caso de uso.

```csharp
internal class GetAuditLogsByActionHandler(IGetAuditLogsByActionRepository repository) : IGetAuditLogsByActionInputPort
{
    public async Task<IEnumerable<AuditLogResponse>> HandleAsync(string id, string action, DateTime? from, DateTime? end)
    {
        DateTime UtcFrom = from != null ? from.Value.Date
            : DateTime.UtcNow.Date.AddDays(-30);

        DateTime UtcEnd = end != null ? end.Value.Date.AddDays(1).AddTicks(-1)
            : DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

        return await repository.GetAuditLogsByActionAsync(id, action, UtcFrom, UtcEnd);
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

# Caso de uso: GetAuditLogsByEntityId
El caso de uso GetAuditLogsByEntityId es responsable de obtener todos AuditLogs del sistema que coincidan con un entityId específico para una compañía dada.

## Parametros de entrada.
- companyId (obligatorio): Identificador de la empresa cuyos registros se desean consultar.
- entityId (obligatorio): Identificador de la entidad asociada al registro de auditoría.
- from (opcional): Fecha de inicio del rango. Si no se especifica, se toma como valor predeterminado 30 días antes del día actual.
- end (opcional): Fecha de fin del rango. Si no se especifica, se toma como valor predeterminado el final del día actual.


## Endpoint REST
Este endpoint permite obtener los logs desde un cliente HTTP.

```csharp
public static class EndpointsMapper
{
    public static IEndpointRouteBuilder MapAuditLogEndpoints(
        this IEndpointRouteBuilder builder)
    {
        builder.MapGet(("{companyId}/" + "Entity" + "/{entityId}").CreateEndpoint("AuditLogEndpoints"), async (
        string companyId,
        string entityId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? end,
        IGetAuditLogsByEntityIdInputPort inputPort) =>
        {
            var result = await inputPort.HandleAsync(companyId, entityId, from, end);
            return TypedResults.Ok(result);

        });
    }
}
```
### Reponse: AuditLogResponse

```csharp
public class AuditLogResponse(int logId, string entityId, string action,
    string performedBy, DateTime timeStamp, DateTime createdAt, string details)
{
    public int LogId => logId;
    public string EntityId => entityId;
    public string Action => action;
    public string PerformedBy => performedBy;
    public DateTime TimeStamp => timeStamp;
    public DateTime CreatedAt => createdAt;
    public string Details => details;
}
```
## Caso de uso: IGetAuditLogsByEntityIdInputPort

```csharp
public interface IGetAuditLogsByEntityIdInputPort
{
    Task<IEnumerable<AuditLogResponse>> HandleAsync(string id, string entityId, DateTime? from, DateTime? end);
}
```

### Implementación del Caso de uso.

```csharp
internal class GetAuditLogsByEntityIdHandler
    (IGetAuditLogsByEntityIdRepository repository) : IGetAuditLogsByEntityIdInputPort
{
    public async Task<IEnumerable<AuditLogResponse>>
        HandleAsync(string id, string entityId, DateTime? from, DateTime? end)
    {

        DateTime UtcFrom = from != null ? from.Value.Date
            : DateTime.UtcNow.Date.AddDays(-30);

        DateTime UtcEnd = end != null ? end.Value.Date.AddDays(1).AddTicks(-1)
            : DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

        return await repository.GetAuditLogsByEntityIdAsync(id, entityId, UtcFrom, UtcEnd);

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


