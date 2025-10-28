# Entidad: NonConformity
Permitir que, ante errores detectados (ya sea en pedidos, productos enviados, pagos, reclamos, etc.), se registre, evalúe y trate cada caso para prevenir su repetición.
La entidad NonConformity está divida en dos, NonConformity y NonConformityDetail. La entidad NonConformity debe incluir:

Id: Identificador único generado por el sistema.

EntityId: Identificador de la entidad afectada.

CompanyId: Identificador de la empresa propietaria del evento.

AffectedProcess: Proceso afectado.

Cause: Causa raíz o explicación inicial del problema detectado.

Status: Estado general del caso de no conformidad.

ReportedAt: Fecha y hora en la que se registró la no conformidad.

NonConformityDetails: Detalles o seguimientos realizados sobre esta no conformidad.

```csharp
public class NonConformity
{
    public Guid Id { get; set; }  
    public string EntityId { get; set; }
    public string CompanyId { get; set; }
    public string AffectedProcess { get; set; }
    public string Cause { get; set; }
    public string Status { get; set; }
    public DateTime ReportedAt { get; set; }
    public List<NonConformityDetail> NonConformityDetails { get; set; }

}
```

## Entidad: NonConformityDetail
Representa un seguimiento, comentario o acción tomada en relación a un caso de no conformidad. Puede haber varios por cada NonConformity. 
La entidad NonConformityDetaill debe incluir:

ReportedBy: Usuario que reportó o documentó este detalle.

Description: Descripción del detalle, acción tomada o comentario.

Status: Descripción del detalle, acción tomada o comentario.

ReportedAt: Fecha y hora en la que se agregó este detalle.

```csharp
public class NonConformityDetail
{
    public string ReportedBy { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public DateTime ReportedAt { get; set; }
}
```
## Repository: Interfaces
En esta sección se definen las interfaces que se utilizarán en los repositorios de los casos de uso de la entidad NonConformity. Siguiendo el patrón CQRS, se separan las operaciones de lectura y escritura en dos interfaces diferentes.

### ICommandNonConformityRepository
La interfaz ICommandNonConformityRepository se encarga únicamente de agregar y guardar registros NonConformity.

```csharp
public interface ICommandNonConformityRepository
{
    Task RegisterNonConformityAsync(NonConformityDto nonConformityDto);
    Task UpdateStatusNonConformityMasterAsync(Guid entityId, string status);
    Task SaveChangesAsync();
}
```

### ICommandNonConformityDetailRepository
La interfaz ICommandNonConformityDetailRepository se encarga únicamente de agregar y guardar registros NonConformityDetail.

```csharp
public interface ICommandNonConformityDetailRepository
{
    Task RegisterNonConformityDetailAsync(NonConformityCreateDetailDto nonConformityDetail);
    Task SaveChangesAsync();
}
```

### IQueryableNonConformityRepository
La interfaz IQueryableNonConformityRepository está dedicada a las operaciones de consulta.

```csharp
public interface IQueryableNonConformityRepository
{
    Task<IEnumerable<NonConformityMaterResponse>> GetAllNonConformitiesAsync(string id, DateTime? from, DateTime? end);
    Task<IEnumerable<NonConformityMaterResponse>> GetNonConformityByAffectedProcesssAsync(string id, string affectedProcess, DateTime? from, DateTime? end);
    Task<IEnumerable<NonConformityResponse>> GetNonConformityByEntityIdAsync(string id, string entityId, DateTime? from, DateTime? end);
    Task<IEnumerable<NonConformityMaterResponse>> GetNonConformityByStatusAsync(string id, string status, DateTime? from, DateTime? end);
    Task<bool> NonConformityExistsByGuidAsync(Guid entityId);
}
```

## Implementación de los repositorios.

### CommandNonConformityRepository
```csharp
internal class CommandNonConformityRepository(
    IWritableNonConformityDataContext commandDataContext,
    IQueryableNonConformityDataContext queryDataContext) : ICommandNonConformityRepository
{
    public async Task RegisterNonConformityAsync(NonConformityDto nonConformityDto)
    {
        Entities.NonConformity NewNonConformityMaster = new Entities.NonConformity
        {
            Id = Guid.NewGuid(),
            ReportedAt = nonConformityDto.ReportedAt,
            CompanyId = nonConformityDto.CompanyId,
            EntityId = nonConformityDto.EntityId,
            AffectedProcess = nonConformityDto.AffectedProcess,
            Cause = nonConformityDto.Cause,
            Status = nonConformityDto.Status.ToLower(),
            NonConformityDetails = new List<NonConformityDetail>()
        };

        NonConformityDetail NewNonConformityDetail = new NonConformityDetail
        {
            ReportedAt = nonConformityDto.ReportedAt,
            ReportedBy = nonConformityDto.ReportedBy,
            Description = nonConformityDto.Description,
            Status = nonConformityDto.Status.ToLower()
        };

        NewNonConformityMaster.NonConformityDetails.Add(NewNonConformityDetail);
        await commandDataContext.AddNonConformityAsync(NewNonConformityMaster);
        await commandDataContext.AddNonConformityDetailAsync(NewNonConformityDetail, NewNonConformityMaster.Id);
    }

    public async Task SaveChangesAsync() => await commandDataContext.SaveChangesAsync();

    public Task UpdateStatusNonConformityMasterAsync(Guid entityId, string status)
    {
        NonConformityReadModel NonConformityMaster = queryDataContext.NonConformities
            .FirstOrDefault(nonConformity =>
                nonConformity.Id == entityId);

        NonConformityMaster.Status = status.ToLower();
        commandDataContext.UpdateNonConformityAsync(NonConformityMaster);
        return Task.CompletedTask;
    }
}
```
### CommandNonConformityDetailRepository
```csharp
internal class CommandNonConformityDetailRepository(
    IWritableNonConformityDataContext dataContext) : ICommandNonConformityDetailRepository
{

    public async Task RegisterNonConformityDetailAsync(NonConformityCreateDetailDto nonConformityDetail)
    {

        NonConformityDetail NewDetail = new NonConformityDetail
        {
            ReportedBy = nonConformityDetail.ReportedBy,
            Description = nonConformityDetail.Description,
            Status = nonConformityDetail.Status.ToLower(),
            ReportedAt = nonConformityDetail.ReportedAt
        };

        await dataContext.AddNonConformityDetailAsync(NewDetail, nonConformityDetail.EntityId);
    }
    public async Task SaveChangesAsync() => await dataContext.SaveChangesAsync();
}
```


### QueryableNonConformityRepository
```csharp
internal class QueryableNonConformityRepository(
    IQueryableNonConformityDataContext dataContext) : IQueryableNonConformityRepository
{
    public async Task<IEnumerable<NonConformityMaterResponse>> GetAllNonConformitiesAsync(string id, DateTime? from, DateTime? end)
    {
        var Query = dataContext.NonConformities
            .Where(NonConformity =>
                NonConformity.CompanyId == id &&
                NonConformity.ReportedAt >= from &&
                NonConformity.ReportedAt <= end)
            .OrderBy(NonConformity => NonConformity.ReportedAt);

        var NonConformities = await dataContext.ToListAsync(Query);

        return NonConformities.Select(
            NonConformity => new NonConformityMaterResponse(
                NonConformity.Id,
                NonConformity.EntityId,
                NonConformity.ReportedAt,
                NonConformity.AffectedProcess,
                NonConformity.Cause,
                NonConformity.Status,
                dataContext.NonConformityDetails.Count(NonConformityDetail =>
                    NonConformityDetail.NonConformityId == NonConformity.Id)));
    }

    public async Task<IEnumerable<NonConformityMaterResponse>> GetNonConformityByAffectedProcesssAsync(string id, string affectedProcess,
        DateTime? from, DateTime? end)
    {
        var Query = dataContext.NonConformities
            .Where(NonConformity =>
                NonConformity.CompanyId == id &&
                NonConformity.AffectedProcess == affectedProcess &&
                NonConformity.ReportedAt >= from &&
                NonConformity.ReportedAt <= end)
            .OrderBy(NonConformity => NonConformity.ReportedAt);

        var NonConformities = await dataContext.ToListAsync(Query);

        return NonConformities.Select(
            NonConformity => new NonConformityMaterResponse(
                NonConformity.Id,
                NonConformity.EntityId,
                NonConformity.ReportedAt,
                NonConformity.AffectedProcess,
                NonConformity.Cause,
                NonConformity.Status,
                dataContext.NonConformityDetails.Count(NonConformityDetail =>
                    NonConformityDetail.NonConformityId == NonConformity.Id)));
    }

    public async Task<IEnumerable<NonConformityResponse>> GetNonConformityByEntityIdAsync(string id, string entityId, DateTime? from, DateTime? end)
    {
        var NonConformities = await dataContext.ToListAsync(
            dataContext.NonConformities
                .Where(NonConformity => NonConformity.CompanyId == id && NonConformity.Id.ToString() == entityId)
        );

        var NonConformityDetails = await dataContext.ToListAsync(
            dataContext.NonConformityDetails
                .Where(d =>
                    d.NonConformityId.ToString() == entityId &&
                    d.ReportedAt >= from &&
                    d.ReportedAt <= end)
                );

        return NonConformities
            .Select(NonConformity => new NonConformityResponse(
                NonConformity.ReportedAt,
                NonConformity.AffectedProcess,
                NonConformity.Status,
                NonConformity.Cause,
                NonConformityDetails
                    .Where(Detail => Detail.NonConformityId == NonConformity.Id)
                    .OrderBy(Detail => Detail.ReportedAt)
                    .Select(Detail => new NonConformityDetailResponse(
                        Detail.ReportedAt,
                        Detail.ReportedBy,
                        Detail.Description,
                        Detail.Status))
                    .ToList()
            ))
            .ToList();
    }

    public async Task<IEnumerable<NonConformityMaterResponse>> GetNonConformityByStatusAsync(string id, string status, DateTime? from, DateTime? end)
    {
        var Query = dataContext.NonConformities
            .Where(NonConformity =>
                NonConformity.CompanyId == id &&
                NonConformity.Status == status &&
                NonConformity.ReportedAt >= from &&
                NonConformity.ReportedAt <= end)
            .OrderBy(NonConformity => NonConformity.ReportedAt);

        var NonConformities = await dataContext.ToListAsync(Query);

        return NonConformities.Select(
            NonConformity => new NonConformityMaterResponse(
                NonConformity.Id,
                NonConformity.EntityId,
                NonConformity.ReportedAt,
                NonConformity.AffectedProcess,
                NonConformity.Cause,
                NonConformity.Status,
                dataContext.NonConformityDetails.Count(NonConformityDetail =>
                    NonConformityDetail.NonConformityId == NonConformity.Id)));
    }

    public Task<bool> NonConformityExistsByGuidAsync(Guid entityId)
    {
        NonConformityReadModel NonConformityMaster = dataContext.NonConformities
            .FirstOrDefault(nonConformity =>
                nonConformity.Id == entityId);

        bool Exists = NonConformityMaster != null;
        return Task.FromResult(Exists);
    }
}
```

## DataContext: Interfaces

En esta sección se definirán las interfaces que se utilizarán en los repositorios de los diferentes casos de usos de la entidad Nonconformity. Siguiendo el patrón CQRS, se separaron las operaciones de lectura y escritura en dos interfaces diferentes.

### IWritableNonConformityDataContext

La interfaz IWritableNonConformityDataContext se encarga exclusivamente de agregar registros de NonConformity, NonConformityDetail, actualizar el registro de NonConformity y guardar cambios.

```csharp
public interface IWritableNonConformityDataContext
{
    Task AddNonConformityAsync(NonConformity nonConformityMaster);
    Task AddNonConformityDetailAsync(NonConformityDetail nonConformityDetail, Guid id);
    Task UpdateNonConformityAsync(NonConformityReadModel nonConformity);
    Task SaveChangesAsync();
}
```

### IQueryableNonConformityDataContext

La interfaz IQueryableNonConformityDataContext está dedicada a las operaciones de lectura, con un IQueryable que expone los registros no conformidad y sus detalles respectivamente. Además
de un método por cada entidad para obtener los resultados.

```csharp
public interface IQueryableNonConformityDataContext
{
    IQueryable<NonConformityReadModel> NonConformities { get; }

    IQueryable<NonConformityDetailReadModel> NonConformityDetails { get; }

    Task<IEnumerable<NonConformityReadModel>> ToListAsync(
        IQueryable<NonConformityReadModel> queryable);

    Task<IEnumerable<NonConformityDetailReadModel>> ToListAsync(
        IQueryable<NonConformityDetailReadModel> queryable);
}
```

## Implementación de los DataContext
Puedes implementar ambos contextos de datos utilizando un sistema de base de datos o almacenamiento de archivos. A continuación se presenta un ejemplo simple de una implementación en memoria para la persistencia de datos.

### InMemoryNonConformityStore

```csharp
internal class InMemoryNonConformityStore
{
    public List<Entities.NonConformity> NonConformities { get; } = new();
    public List<Entities.NonConformityDetail> NonConformityDetails { get; } = new();
    public int NonConformityDetailsCurrentId { get; set; }

}
```

### InMemoryWritableNonConformityDataContext

```csharp
internal class InMemoryWritableNonConformityDataContext(
    InMemoryNonConformityStore dataContext) : IWritableNonConformityDataContext
{
    public Task AddNonConformityAsync(Repositories.NonConformityRepositories.Entities.NonConformity nonConformityMaster)
    {
        var NonConformityRecord = new DataContexts.Entities.NonConformity
        {
            Id = nonConformityMaster.Id,
            ReportedAt = nonConformityMaster.ReportedAt,
            EntityId = nonConformityMaster.EntityId,
            CompanyId = nonConformityMaster.CompanyId,
            AffectedProcess = nonConformityMaster.AffectedProcess,
            Cause = nonConformityMaster.Cause,
            Status = nonConformityMaster.Status,
            CreatedAt = DateTime.UtcNow
        };
        dataContext.NonConformities.Add(NonConformityRecord);
        return Task.CompletedTask;
    }

    public Task AddNonConformityDetailAsync(Repositories.NonConformityRepositories.Entities.NonConformityDetail nonConformityDetail, Guid id)
    {
        var NonConformity = dataContext.NonConformities
            .FirstOrDefault(nonConformity =>
            nonConformity.Id == id);

        var NonConformityDetailRecord = new DataContexts.Entities.NonConformityDetail
        {
            Id = ++dataContext.NonConformityDetailsCurrentId,
            NonConformityId = NonConformity.Id,
            ReportedAt = nonConformityDetail.ReportedAt,
            ReportedBy = nonConformityDetail.ReportedBy,
            Description = nonConformityDetail.Description,
            Status = nonConformityDetail.Status,
            CreatedAt = DateTime.UtcNow
        };

        dataContext.NonConformityDetails.Add(NonConformityDetailRecord);
        return Task.CompletedTask;
    }

    public Task UpdateNonConformityAsync(NonConformityReadModel nonConformityUpdated)
    {
        var NonConformitRecord = dataContext.NonConformities
            .FirstOrDefault(NonConformity => NonConformity.Id == nonConformityUpdated.Id);

        NonConformitRecord.EntityId = nonConformityUpdated.EntityId;
        NonConformitRecord.CompanyId = nonConformityUpdated.CompanyId;
        NonConformitRecord.AffectedProcess = nonConformityUpdated.AffectedProcess;
        NonConformitRecord.Cause = nonConformityUpdated.Cause;
        NonConformitRecord.Status = nonConformityUpdated.Status;

        return Task.CompletedTask;
    }


    public Task SaveChangesAsync()
    {
        return Task.CompletedTask;
    }
}
```

### InMemoryQueryableNonConformityDataContext

```csharp
internal class InMemoryQueryableNonConformityDataContext(
    InMemoryNonConformityStore dataContext) : IQueryableNonConformityDataContext
{
    public IQueryable<NonConformityReadModel> NonConformities =>
        dataContext.NonConformities
        .Select(NonConformity => new NonConformityReadModel
        {
            Id = NonConformity.Id,
            ReportedAt = NonConformity.ReportedAt,
            EntityId = NonConformity.EntityId,
            CompanyId = NonConformity.CompanyId,
            AffectedProcess = NonConformity.AffectedProcess,
            Cause = NonConformity.Cause,
            Status = NonConformity.Status,
            CreatedAt = NonConformity.CreatedAt
        }).AsQueryable();

    public IQueryable<NonConformityDetailReadModel> NonConformityDetails =>
    dataContext.NonConformityDetails
    .Select(NonConformityDetail => new NonConformityDetailReadModel
    {
        Id = NonConformityDetail.Id,
        ReportedAt = NonConformityDetail.ReportedAt,
        ReportedBy = NonConformityDetail.ReportedBy,
        Description = NonConformityDetail.Description,
        Status = NonConformityDetail.Status,
        CreatedAt = NonConformityDetail.CreatedAt,
        NonConformityId = NonConformityDetail.NonConformityId
    }).AsQueryable();

    public async Task<IEnumerable<NonConformityReadModel>> ToListAsync(IQueryable<NonConformityReadModel> queryable)
        => await Task.FromResult(queryable.ToList());

    public async Task<IEnumerable<NonConformityDetailReadModel>> ToListAsync(IQueryable<NonConformityDetailReadModel> queryable)
        => await Task.FromResult(queryable.ToList());
}
```

# Caso de uso: RegisterNonConformity
El caso de uso RegisterNonConformity es responsable de registrar la entidad NonConformity junto a un detalle inicial en el sistema.

## Parametros de Entrada.
- NonConformityRequest (obligatorio).

## Endpoint REST
Este endpoint permite registrar la entidad NonConformity junto a su primer detallle desde un cliente HTTP.

```csharp
public static class EndpointsMapper
{
    public static IEndpointRouteBuilder MapNonConformityEndpoints(
        this IEndpointRouteBuilder builder)
    {
        builder.MapPost("".CreateEndpoint("NonConformityEndpoints"),
        async (NonConformityRequest nonConformity, IRegisterNonConformityInputPort inputPort) =>
        {

            await inputPort.HandleAsync(new NonConformityDto(
                nonConformity.EntityId,
                nonConformity.CompanyId,
                nonConformity.ReportedAt,
                nonConformity.ReportedBy,
                nonConformity.Description,
                nonConformity.AffectedProcess,
                nonConformity.Cause,
                nonConformity.Status
                ));
            return TypedResults.Created();
        });
    }
}
```
### DTO y Request

```csharp
public class NonConformityDto(string entityId, string companyId, DateTime reportedAt,
    string reportedBy, string description, string affectedProcess, string cause, string status)
{
    public string EntityId => entityId;
    public string CompanyId => companyId;
    public DateTime ReportedAt => reportedAt;
    public string ReportedBy => reportedBy;
    public string Description => description;
    public string AffectedProcess => affectedProcess;
    public string Cause => cause;
    public string Status => status;
}
```

```csharp
public class NonConformityRequest
{
    public string EntityId { get; set; }
    public string CompanyId { get; set; }
    public DateTime ReportedAt { get; set; }
    public string ReportedBy { get; set; }
    public string Description { get; set; }
    public string AffectedProcess { get; set; }
    public string Cause { get; set; }
    public string Status { get; set; }
}
```

## Caso de uso: IRegisterNonConformityInputPort

```csharp
public interface IRegisterNonConformityInputPort
{
    Task HandleAsync(NonConformityDto nonConformityDto);
}
```

### Implementación del Caso de uso.
```csharp
internal class RegisterNonConformityHandler
    (IRegisterNonConformityRepository repository) : IRegisterNonConformityInputPort
{
    public async Task HandleAsync(NonConformityDto nonConformityDto)
    {
        await repository.RegisterNonConformityAsync(nonConformityDto);
        await repository.SaveChangesAsync();
    }
}
```

# Integración en Blazor WebAssembly (UI)
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

# Caso de uso: GetAllNonConformities
El caso de uso GetAllNonConformities es responsable de obtener todos los registros no conformidad del sistema para una compañía específica dentro de un rango de fechas.

## Parametros de entrada.
- companyId (obligatorio): Identificador de la empresa cuyos registros se desean consultar.
- from (opcional): Fecha de inicio del rango. Si no se especifica, se toma como valor predeterminado 30 días antes del día actual.
- end (opcional): Fecha de fin del rango. Si no se especifica, se toma como valor predeterminado el final del día actual.


## Endpoint REST
Este endpoint permite obtener los registros de no conformidad desde un cliente HTTP.

```csharp
public static class EndpointsMapper
{
    public static IEndpointRouteBuilder MapNonConformityEndpoints(
        this IEndpointRouteBuilder builder)
    {
            builder.MapGet("{companyId}/".CreateEndpoint("NonConformityEndpoints"), async (
                string companyId,
                [FromQuery] DateTime? from,
                [FromQuery] DateTime? end,
                IGetAllNonConformitiesInputPort inputPort) =>
            {
                var result = await inputPort.HandleAsync(companyId, from, end);
                return TypedResults.Ok(result);
            });
    }
}
```
### Reponse: NonConformityMaterResponse

```csharp
public class NonConformityMaterResponse(Guid id, string entityId, DateTime reportedAt,
    string affectedProcess, string cause, string status, int detailsCount)
{
    public Guid Id => id;
    public string EntityId => entityId;
    public DateTime ReportedAt => reportedAt;
    public string AffectedProcess => affectedProcess;
    public string Cause => cause;
    public string Status => status;
    public int DetailsCount => detailsCount;
}
```

## Caso de uso: IGetAllNonConformitiesInputPort

```csharp
public interface IGetAllNonConformitiesInputPort
{
    Task<IEnumerable<NonConformityMaterResponse>> HandleAsync(string id, DateTime? from, DateTime? end);
}
```

### Implementación del Caso de uso.

```csharp
internal class GetAllNonConformitiesHandler(IGetAllNonConformitiesRepository repository): IGetAllNonConformitiesInputPort
{
    public async Task<IEnumerable<NonConformityMaterResponse>> HandleAsync(string id, DateTime? from, DateTime? end)
    {
        DateTime UtcFrom = from != null ? from.Value.Date
            : DateTime.UtcNow.Date.AddDays(-30);

        DateTime UtcEnd = end != null ? end.Value.Date.AddDays(1).AddTicks(-1)
            : DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

        return await repository.GetAllNonConformitiesAsync(id, UtcFrom, UtcEnd);
    }
}
```

# Integración en Blazor WebAssembly (UI)
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

# Caso de uso: GetNonConformitiesByAffectedProcess
El caso de uso GetNonConformitiesByAffectedProcess es responsable de obtener todos los registros no conformidad del sistema según el proceso afectado para una compañía específica dentro de un rango de fechas.

## Parametros de entrada.
- companyId (obligatorio): Identificador de la empresa cuyos registros se desean consultar.
- affectedProcess (obligatorio): Proceso afectado.
- from (opcional): Fecha de inicio del rango. Si no se especifica, se toma como valor predeterminado 30 días antes del día actual.
- end (opcional): Fecha de fin del rango. Si no se especifica, se toma como valor predeterminado el final del día actual.

## Endpoint REST
Este endpoint permite obtener los registros de no conformidad desde un cliente HTTP.

```csharp
public static class EndpointsMapper
{
    public static IEndpointRouteBuilder MapNonConformityEndpoints(
        this IEndpointRouteBuilder builder)
    {
        builder.MapGet(("{companyId}/" + "AffectedProcess"+ "/{affectedProcess}").CreateEndpoint("NonConformityEndpoints"), async (
            string companyId,
            string affectedProcess,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? end,
            IGetNonConformityByAffectedProcessInputPort inputPort) =>
        {
            var result = await inputPort.HandleAsync(companyId, affectedProcess, from, end);
            return TypedResults.Ok(result);
        });
}
}
```
### Reponse: NonConformityMaterResponse

```csharp
public class NonConformityMaterResponse(Guid id, string entityId, DateTime reportedAt,
    string affectedProcess, string cause, string status, int detailsCount)
{
    public Guid Id => id;
    public string EntityId => entityId;
    public DateTime ReportedAt => reportedAt;
    public string AffectedProcess => affectedProcess;
    public string Cause => cause;
    public string Status => status;
    public int DetailsCount => detailsCount;
}
```

## Caso de uso: IGetNonConformityByAffectedProcessInputPort

```csharp
public interface IGetNonConformityByAffectedProcessInputPort
{
    Task<IEnumerable<NonConformityMaterResponse>> HandleAsync(string id, string affectedProcess, DateTime? from,  DateTime? end);
}
```

### Implementación del Caso de uso.

```csharp
internal class GetNonConformityByAffectedProcessHandler
    (IGetNonConformityByAffectedProcessRepository repository): IGetNonConformityByAffectedProcessInputPort
{
    public async Task<IEnumerable<NonConformityMaterResponse>> HandleAsync(string id, string affectedProcess, DateTime? from, DateTime? end)
    {
        DateTime UtcFrom = from != null ? from.Value.Date
            : DateTime.UtcNow.Date.AddDays(-30);

        DateTime UtcEnd = end != null ? end.Value.Date.AddDays(1).AddTicks(-1)
            : DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

        return await repository.GetNonConformityByAffectedProcesssAsync(id, affectedProcess, UtcFrom, UtcEnd);
    }
}
```

# Integración en Blazor WebAssembly (UI)
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

# Caso de uso: GetNonConformityByStatus
El caso de uso GetNonConformityByStatus es responsable de obtener todos los registros no conformidad del sistema según su status para una compañía específica dentro de un rango de fechas.

## Parametros de entrada.
- companyId (obligatorio): Identificador de la empresa cuyos registros se desean consultar.
- status (obligatorio): estatus del registro.
- from (opcional): Fecha de inicio del rango. Si no se especifica, se toma como valor predeterminado 30 días antes del día actual.
- end (opcional): Fecha de fin del rango. Si no se especifica, se toma como valor predeterminado el final del día actual.

## Endpoint REST
Este endpoint permite obtener los registros de no conformidad desde un cliente HTTP.

```csharp
public static class EndpointsMapper
{
    public static IEndpointRouteBuilder MapNonConformityEndpoints(
        this IEndpointRouteBuilder builder)
    {
            builder.MapGet(("{companyId}/" + "Status" + "/{status}").CreateEndpoint("NonConformityEndpoints"), async (
                string companyId,
                string status,
                [FromQuery] DateTime? from,
                [FromQuery] DateTime? end,
                IGetNonConformityByStatusInputPort inputPort) =>
            {
                var result = await inputPort.HandleAsync(companyId, status, from, end);
                return TypedResults.Ok(result);

            });
    }
}
```
### Reponse: NonConformityMaterResponse

```csharp
public class NonConformityMaterResponse(Guid id, string entityId, DateTime reportedAt,
    string affectedProcess, string cause, string status, int detailsCount)
{
    public Guid Id => id;
    public string EntityId => entityId;
    public DateTime ReportedAt => reportedAt;
    public string AffectedProcess => affectedProcess;
    public string Cause => cause;
    public string Status => status;
    public int DetailsCount => detailsCount;
}
```

## Caso de uso: IGetNonConformityByStatusInputPort

```csharp
public interface IGetNonConformityByStatusInputPort
{
    Task<IEnumerable<NonConformityMaterResponse>> HandleAsync(string id, string status, DateTime? from, DateTime? end);

}
```

### Implementación del Caso de uso.

```csharp
internal class GetNonConformityByStatusHandler(
    IGetNonConformityByStatusRepository repository) : IGetNonConformityByStatusInputPort
{
    public async Task<IEnumerable<NonConformityMaterResponse>> HandleAsync(string id, string status, DateTime? from, DateTime? end)
    {
        DateTime UtcFrom = from != null ? from.Value.Date
            : DateTime.UtcNow.Date.AddDays(-30);

        DateTime UtcEnd = end != null ? end.Value.Date.AddDays(1).AddTicks(-1)
            : DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

        return await repository.GetNonConformityByStatusAsync(id, status, UtcFrom, UtcEnd);
    }
}
```

# Integración en Blazor WebAssembly (UI)
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


# Caso de uso: GetNonConformityByEntityId
Este caso de uso permite obtener un registro de no conformidad específico, identificado por su EntityId, junto con todos sus detalles asociados. La búsqueda se limita a una compañía determinada y a un rango de fechas definido, asegurando que solo se recuperen los casos reportados dentro del período indicado.

## Parametros de entrada.
- companyId (obligatorio): Identificador de la empresa cuyo registro se desean consultar.
- entityId (obligatorio): Id del registro de no conformidad.
- from (opcional): Fecha de inicio del rango. Si no se especifica, se toma como valor predeterminado 30 días antes del día actual.
- end (opcional): Fecha de fin del rango. Si no se especifica, se toma como valor predeterminado el final del día actual.

## Endpoint REST
Este endpoint permite obtener el registro de no conformidad y sus detalles desde un cliente HTTP.

```csharp
public static class EndpointsMapper
{
    public static IEndpointRouteBuilder MapNonConformityEndpoints(
        this IEndpointRouteBuilder builder)
    {
        builder.MapGet(("{companyId}/" + "Entity" + "/{entityId}").CreateEndpoint("NonConformityEndpoints"), async (
            string companyId,
            string entityId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? end,
            IGetNonConformityByEntityIdInputPort inputPort) =>
        {
            var result = await inputPort.HandleAsync(companyId, entityId, from, end);
            return TypedResults.Ok(result);

        });
    }
}
```
### Reponse: NonConformityResponse

```csharp
public class NonConformityResponse(DateTime repotedAt, string affectedProcess,
    string status, string cause, List<NonConformityDetailResponse> details)
{
    public DateTime ReportedAt => repotedAt;
    public string AffectedProcess => affectedProcess;
    public string Status => status;
    public string Cause => cause;
    public List<NonConformityDetailResponse> Details => details;

}
```
## Caso de uso: IGetNonConformityByEntityIdInputPort

```csharp
public interface IGetNonConformityByEntityIdInputPort
{
    Task<IEnumerable<NonConformityResponse>> HandleAsync(string id, string entityId, DateTime? from, DateTime? end);

}
```

### Implementación del Caso de uso.

```csharp
internal class GetNonConformityByEntityIdHandler(
    IGetNonConformityByEntityIdRepository repository) : IGetNonConformityByEntityIdInputPort
{
    public async Task<IEnumerable<NonConformityResponse>> HandleAsync(string id, string entityId, DateTime? from, DateTime? end)
    {
        DateTime UtcFrom = from != null ? from.Value.Date
            : DateTime.UtcNow.Date.AddDays(-30);

        DateTime UtcEnd = end != null ? end.Value.Date.AddDays(1).AddTicks(-1)
            : DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

        return await repository.GetNonConformityByEntityIdAsync(id, entityId, UtcFrom, UtcEnd);
    }
}
```

# Integración en Blazor WebAssembly (UI)
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
