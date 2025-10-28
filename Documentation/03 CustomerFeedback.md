# Entidad: CustomerFeedback
Este proceso puede estar vinculado a la finalización del pedido (PlaceOrder) y tener efectos como:

- Mostrar un formulario de satisfacción.
- Enviar un correo solicitando feedback.
- Guardar el feedback para análisis futuro (otro principio clave de ISO 9001).

La entidad CustomerFeedback almacenará los registros de satisfacción del cliente. Este registro deberá incluir.

EntityId: Identificador de la entidad afectada.

CompanyId: Identificador de la empresa propietaria del evento.

CustomerId: El usuario que ha realizado la acción (ID del cliente).

Rating: Calificación de satisfacción (escala del 1 al 5).

Comments: Comentarios adicionales acerca de la satisfacción del cliente.

ReportedAt: La fecha y hora en la que se reportó.

```csharp
public class CustomerFeedback()
{
    public string EntityId { get; set; }
    public string CompanyId { get; set; }
    public string CustomerId { get; set; }
    public int Rating { get; set; }
    public string Comments { get; set; }
    public DateTime ReportedAt { get; set; }
}
```
## Repository: Interfaces
En esta sección se definen las interfaces que se utilizarán en los repositorios de los casos de uso de la entidad CustomerFeedback. Siguiendo el patrón CQRS, se separan las operaciones de lectura y escritura en dos interfaces diferentes.

### ICommandCustomerFeedbackRepository
La interfaz ICommandCustomerFeedbackRepository se encarga únicamente de agregar y guardar registros CustomerFeedback.

```csharp
public interface ICommandCustomerFeedbackRepository
{
    Task RegisterCustomerFeedbackAsync(CustomerFeedbackDto customerFeedbackDto);
    Task SaveChangesAsync();

}
```

### IQueryableCustomerFeedbackRepository
La interfaz IQueryableCustomerFeedbackRepository está dedicada a las operaciones de consulta.

```csharp
public interface IQueryableCustomerFeedbackRepository
{
    Task<IEnumerable<CustomerFeedbackResponse>> GetCustomerFeedbackByRatingAsync(string id, int rating, DateTime? from, DateTime? end);

    Task<CustomerFeedbackResponse> GetCustomerFeedbackByIdAsync(string companyId, int id);

    Task<bool> CustomerFeedbackExists(string companyId, int id);

    Task<IEnumerable<CustomerFeedbackResponse>> GetCustomerFeedbackByEntityId(string id, string entityId);

    Task<IEnumerable<CustomerFeedbackResponse>> GetCustomerFeedbackByCustomerIdAsync(string id, string customerId, DateTime? from, DateTime? end);

    Task<IEnumerable<CustomerFeedbackResponse>> GetAllCustomerFeedbacksAsync(string id, DateTime? from, DateTime? end);
}
```

## Implementación de los repositorios.

### CommandCustomerFeedbackRepository
```csharp
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
```

### QueryableCustomerFeedbackRepository
```csharp
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
```



## DataContext: Interfaces

En esta sección se definirán las interfaces que se utilizarán en los repositorios de los diferentes casos de usos de la entidad CustomerFeedback. Siguiendo el patrón CQRS, se separaron las operaciones de lectura y escritura en dos interfaces diferentes.

### IWritableCustomerFeedbackDataContext

La interfaz IWritableCustomerFeedbackDataContext se encarga exclusivamente de agregar y guardar los registros.

```csharp
public interface IWritableCustomerFeedbackDataContext
{
    Task AddAsync(Entities.CustomerFeedback customerFeedback);
    Task SaveChangesAsync();
}
```

### IQueryableCustomerFeedbackDataContext

La interfaz IQueryableCustomerFeedbackDataContext está dedicada a las operaciones de lectura, con un IQueryable que expone los registros  de satisfacción y un método para obtener los resultados.

```csharp
public interface IQueryableCustomerFeedbackDataContext
{
    IQueryable<CustomerFeedbackReadModel> CustomerFeedbacks { get; }
    Task<IEnumerable<CustomerFeedbackReadModel>> ToListAsync(
        IQueryable<CustomerFeedbackReadModel> queryable);
}
```

## Implementación de los DataContext
Puedes implementar ambos contextos de datos utilizando un sistema de base de datos o almacenamiento de archivos. A continuación se presenta un ejemplo simple de una implementación en memoria para la persistencia de datos.

### InMemoryCustomerFeedbackStore

```csharp
internal class InMemoryCustomerFeedbackStore
{
    public List<Entities.AuditLog> AuditLogs { get; } = new();
    public int CurrentId { get; set; }
}
```


### InMemoryWritableCustomerFeedbackDataContext

```csharp
internal class InMemoryWritableCustomerFeedbackDataContext(
    InMemoryCustomerFeedbackStore dataContext) : IWritableCustomerFeedbackDataContext
{
    public Task AddAsync(Repositories.AuditLogRepositories.Entities.AuditLog auditLog)
    {
        var Record = new DataContexts.Entities.AuditLog
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
```

### InMemoryQueryableCustomerFeedbackDataContext

```csharp
internal class InMemoryQueryableCustomerFeedbackDataContext(
    InMemoryCustomerFeedbackStore dataContext) : IQueryableCustomerFeedbackDataContext
{
    public IQueryable<CustomerFeedbackReadModel> CustomerFeedbacks =>
        dataContext.CustomerFeedbacks
        .Select(CustomerFeedback => new CustomerFeedbackReadModel
        {
            Id = CustomerFeedback.Id,
            EntityId = CustomerFeedback.EntityId,
            CompanyId = CustomerFeedback.CompanyId,
            CustomerId = CustomerFeedback.CustomerId,
            Rating = CustomerFeedback.Rating,
            Comments = CustomerFeedback.Comments,
            ReportedAt = CustomerFeedback.ReportedAt,
            CreatedAt = CustomerFeedback.ReportedAt
        }).AsQueryable();


    public async Task<IEnumerable<CustomerFeedbackReadModel>> ToListAsync(IQueryable<CustomerFeedbackReadModel> queryable)
        => await Task.FromResult(queryable.ToList());

}
```
# Caso de uso: RegisterCustomerFeedback
El caso de uso RegisterCustomerFeedback es responsable de registrar la entidad CustomerFeedback en el sistema.

## Parametros de Entrada.
- CustomerFeedbackRequest (obligatorio).
## Endpoint REST
Este endpoint permite registrar CustomerFeedback desde un cliente HTTP.

```csharp
public static class EndpointsMapper
{
    public static IEndpointRouteBuilder MapCustomerFeedbackEndpoints(
        this IEndpointRouteBuilder builder)
    {
        builder.MapPost("".CreateEndpoint("CustomerFeedbackEndpoints"),
            async (CustomerFeedbackRequest customerFeedback, IRegisterCustomerFeedbackInputPort inputport) =>
            {
                await inputport.HandleAsync(new CustomerFeedbackDto(
                    customerFeedback.EntityId,
                    customerFeedback.CompanyId,
                    customerFeedback.CustomerId,
                    customerFeedback.Rating,
                    customerFeedback.Comments,
                    customerFeedback.ReportedAt
                    ));
                return TypedResults.Created();
            });
    }
}
```
### DTO y Request

```csharp
public class CustomerFeedbackDto(
    string entityId, string companyId, string customerId, int rating,
    string comments, DateTime reportedAt)
{
    public string EntityId => entityId;
    public string CompanyId => companyId;
    public string CustomerId => customerId;
    public int Rating => rating;
    public string Comments => comments;
    public DateTime ReportedAt => reportedAt;
}
```

```csharp
public class CustomerFeedbackRequest
{
    public string EntityId { get; set; }
    public string CompanyId { get; set; }
    public string CustomerId { get; set; }
    public int Rating { get; set; }
    public string Comments { get; set; }
    public DateTime ReportedAt { get; set; }
}
```
## Caso de uso: IRegisterCustomerFeedbackInputPort

```csharp
public interface IRegisterCustomerFeedbackInputPort
{
    Task HandleAsync(CustomerFeedbackDto customerFeedbackDto);
}
```

### Implementación del Caso de uso.
El registro de satisfacción deberá contar con un rating entre 1 y 5. De lo contrario, se lanzará una exepción.

```csharp
internal class RegisterCustomerFeedbackHandler(
    IRegisterCustomerFeedbackRepository repository) : IRegisterCustomerFeedbackInputPort
{
    public async Task HandleAsync(CustomerFeedbackDto customerFeedbackDto)
    {
        if (customerFeedbackDto.Rating < 1 || customerFeedbackDto.Rating > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(customerFeedbackDto),
                "Rating must be between 1 and 5.");
        }

        await repository.RegisterCustomerFeedbackAsync(customerFeedbackDto);
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

# Caso de uso: GetAllCustomerFeedbacks
El caso de uso GetAllCustomerFeedbacks es responsable de obtener todos los registros de satisfacción del sistema para una compañía específica dentro de un rango de fechas.

## Parametros de entrada.
- companyId (obligatorio): Identificador de la empresa cuyos registros se desean consultar.
- from (opcional): Fecha de inicio del rango. Si no se especifica, se toma como valor predeterminado 30 días antes del día actual.
- end (opcional): Fecha de fin del rango. Si no se especifica, se toma como valor predeterminado el final del día actual.


## Endpoint REST
Este endpoint permite obtener los registros de satisfacción desde un cliente HTTP.

```csharp
public static class EndpointsMapper
{
    builder.MapGet("{companyId}/".CreateEndpoint("CustomerFeedbackEndpoints"), async (
    string companyId,
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? end,
    IGetAllCustomerFeedbackInputPort inputPort) =>
            {
                var result = await inputPort.HandleAsync(companyId, from, end);
                return TypedResults.Ok(result);

            });
}
```
### Reponse: CustomerFeedbackResponse

```csharp
public class CustomerFeedbackResponse(string entityId, string customerId, int rating, DateTime reportedAt)
{
    public string EntityId => entityId;
    public string CustomerId => customerId;
    public int Rating => rating;
    public DateTime ReportedAt => reportedAt;
}
```

## Caso de uso: IGetAllCustomerFeedbackInputPort

```csharp
public interface IGetAllCustomerFeedbackInputPort
{
    Task<IEnumerable<CustomerFeedbackResponse>> HandleAsync(string id, DateTime? from, DateTime? end);
}
```

### Implementación del Caso de uso.

```csharp
internal class GetAllCustomerFeedbackHandler(
    IGetAllCustomerFeedbackRepository repository) : IGetAllCustomerFeedbackInputPort
{
    public async Task<IEnumerable<CustomerFeedbackResponse>> HandleAsync(string id, DateTime? from, DateTime? end)
    {
        DateTime UtcFrom = from != null ? from.Value.Date
            : DateTime.UtcNow.Date.AddDays(-30);

        DateTime UtcEnd = end != null ? end.Value.Date.AddDays(1).AddTicks(-1)
            : DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

        return await repository.GetAllCustomerFeedbacksAsync(id, UtcFrom, UtcEnd);


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

# Caso de uso: GetCustomerFeedbackByCustomerId
Este caso de uso permite obtener todos los registros de satisfacción (CustomerFeedback) correspondientes a un cliente específico dentro de una compañía, en un rango de fechas determinado.

## Parametros de entrada.
- companyId (obligatorio): Identificador de la empresa cuyos registros se desean consultar.
- customerId (obligatorio): Identificador del cliente cuyos registros se desean obtener.
- from (opcional): Fecha de inicio del rango. Si no se especifica, se toma como valor predeterminado 30 días antes del día actual.
- end (opcional): Fecha de fin del rango. Si no se especifica, se toma como valor predeterminado el final del día actual.


## Endpoint REST
Este endpoint permite obtener los registros de satisfacción desde un cliente HTTP.

```csharp
public static class EndpointsMapper
{
    public static IEndpointRouteBuilder MapCustomerFeedbackEndpoints(
        this IEndpointRouteBuilder builder)
    {
        builder.MapGet(("{companyId}/" + "Customer" + "/{customerId}").CreateEndpoint("CustomerFeedbackEndpoints"), async (
        string companyId,
        string customerId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? end,
        [FromServices] IGetCustomerFeedbackByCustomerIdInputPort inputPort) =>
                {
                    var result = await inputPort.HandleAsync(companyId, customerId, from, end);
                    return TypedResults.Ok(result);

                });
    }
}
```
### Reponse: CustomerFeedbackResponse

```csharp
public class CustomerFeedbackResponse(string entityId, string customerId, int rating, DateTime reportedAt)
{
    public string EntityId => entityId;
    public string CustomerId => customerId;
    public int Rating => rating;
    public DateTime ReportedAt => reportedAt;
}
```

## Caso de uso: IGetCustomerFeedbackByCustomerIdInputPort

```csharp
public interface IGetCustomerFeedbackByCustomerIdInputPort
{
    Task<IEnumerable<CustomerFeedbackResponse>> HandleAsync(string id, string customerId, DateTime? from, DateTime? end);
}
```

### Implementación del Caso de uso.

```csharp
internal class GetCustomerFeedbackByCustomerIdHandler
    (IGetCustomerFeedbackByCustomerIdRepository repository): IGetCustomerFeedbackByCustomerIdInputPort
{
    public async Task<IEnumerable<CustomerFeedbackResponse>> HandleAsync(string id, string customerId, DateTime? from, DateTime? end)
    {
        DateTime UtcFrom = from != null ? from.Value.Date
            : DateTime.UtcNow.Date.AddDays(-30);

        DateTime UtcEnd = end != null ? end.Value.Date.AddDays(1).AddTicks(-1)
            : DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

        return await repository.GetCustomerFeedbackByCustomerIdAsync(id, customerId, UtcFrom, UtcEnd);
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

# Caso de uso: GetCustomerFeedbackByEntityId
Este caso de uso permite obtener todos los registros de satisfacción (CustomerFeedback) asociados a una entidad específica (entityId) dentro de una compañía. El resultado incluye todas las evaluaciones registradas para dicha entidad, sin filtrar por fechas.


## Parametros de entrada.
- companyId (obligatorio): Identificador de la empresa cuyos registros se desean consultar.
- entityId (obligatorio): Identificador de la entidad cuyos registros de satisfacción se desean consultar.

## Endpoint REST
Este endpoint permite obtener los registros de satisfacción desde un cliente HTTP.

```csharp
public static class EndpointsMapper
{
    public static IEndpointRouteBuilder MapCustomerFeedbackEndpoints(
        this IEndpointRouteBuilder builder)
    {
        builder.MapGet(("{companyId}/" + "Entity" + "/{entityId}").CreateEndpoint("CustomerFeedbackEndpoints"), async (
        string companyId,
        string entityId,
        IGetCustomerFeedbackByEntityIdInputPort inputPort) =>
                {
                    var result = await inputPort.HandleAsync(companyId, entityId);
                    return TypedResults.Ok(result);

                });

    }
}
```
### Reponse: CustomerFeedbackResponse

```csharp
public class CustomerFeedbackResponse(string entityId, string customerId, int rating, DateTime reportedAt)
{
    public string EntityId => entityId;
    public string CustomerId => customerId;
    public int Rating => rating;
    public DateTime ReportedAt => reportedAt;
}
```

## Caso de uso: IGetCustomerFeedbackByEntityIdInputPort

```csharp
public interface IGetCustomerFeedbackByEntityIdInputPort
{
    Task<IEnumerable<CustomerFeedbackResponse>> HandleAsync(string id, string entityId);
}
```

### Implementación del Caso de uso.

```csharp
internal class GetGustomerFeedbackByEntityIdHandler(
    IGetCustomerFeedbackByEntityIdRepository repository): IGetCustomerFeedbackByEntityIdInputPort
{
    public async Task<IEnumerable<CustomerFeedbackResponse>> HandleAsync(string id, string entityId)
    {
        return await repository.GetCustomerFeedbackByEntityId(id, entityId);
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

# Caso de uso: GetCustomerFeedbackById
Este caso de uso permite obtener un registro(CustomerFeedback) a partir de un companyId y Id.


## Parametros de entrada.
- companyId (obligatorio): Identificador de la empresa cuyos registros se desean consultar.
- id (obligatorio): Identificador de la entidad c.

## Endpoint REST
Este endpoint permite obtener el correspondiente CustomerFeedback desde un cliente HTTP.

```csharp
public static class EndpointsMapper
{
    public static IEndpointRouteBuilder MapCustomerFeedbackEndpoints(
        this IEndpointRouteBuilder builder)
    {
        builder.MapGet(("{companyId}/" + "Id" + "/{id}").CreateEndpoint("CustomerFeedbackEndpoints"), async (
        string companyId,
        int id,
        IGetCustomerFeedbackByIdInputPort inputport) =>
                {
                    var Result = await inputport.HandleAsync(companyId, id);
                    return TypedResults.Ok(Result);
                });
    }
}
```
### Reponse: CustomerFeedbackResponse

```csharp
public class CustomerFeedbackResponse(string entityId, string customerId, int rating, DateTime reportedAt)
{
    public string EntityId => entityId;
    public string CustomerId => customerId;
    public int Rating => rating;
    public DateTime ReportedAt => reportedAt;
}
```

## Caso de uso: IGetCustomerFeedbackByIdInputPort

```csharp
public interface IGetCustomerFeedbackByIdInputPort
{
    Task<CustomerFeedbackResponse> HandleAsync(string companyId, int id);
}
```

### Implementación del Caso de uso.

```csharp
internal class GetCustomerFeedbackByIdHandler
    (IQueryableCustomerFeedbackRepository repository) : IGetCustomerFeedbackByIdInputPort
{
    public async Task<CustomerFeedbackResponse> HandleAsync(string companyId, int id)
    {
        var CustomerFeedbackExists = await repository.CustomerFeedbackExists(companyId, id);

        if (!CustomerFeedbackExists)
        {
            throw new KeyNotFoundException($"CustomerFeedback with Id '{id}' doesn't exist in the company: '{companyId}'");
        }
        else
        {
            return await repository.GetCustomerFeedbackByIdAsync(companyId, id);
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

# Caso de uso: GetCustomerFeedbackByRating
Este caso de uso permite obtener todos los registros de satisfacción (CustomerFeedback) dentro de una compañía que coincidan con una rating en especifico (1-5), en un rango de fechas determinado.

## Parametros de entrada.
- companyId (obligatorio): Identificador de la empresa cuyos registros se desean consultar.
- rating (obligatorio): Valor numérico de la calificación por la cual se filtrarán los registros (1-5);
- from (opcional): Fecha de inicio del rango. Si no se especifica, se toma como valor predeterminado 30 días antes del día actual.
- end (opcional): Fecha de fin del rango. Si no se especifica, se toma como valor predeterminado el final del día actual.


## Endpoint REST
Este endpoint permite obtener los registros de satisfacción desde un cliente HTTP.

```csharp
public static class EndpointsMapper
{
    public static IEndpointRouteBuilder MapCustomerFeedbackEndpoints(
        this IEndpointRouteBuilder builder)
    {
        builder.MapGet(("{companyId}/" + "Rating" + "/{rating}").CreateEndpoint("CustomerFeedbackEndpoints"), async (
        string companyId,
        int rating,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? end,
        IGetCustomerFeedbackByRatingInputPort inputPort) =>
                {
                    var result = await inputPort.HandleAsync(companyId, rating, from, end);
                    return TypedResults.Ok(result);

                });
    }
}
```
### Reponse: CustomerFeedbackResponse

```csharp
public class CustomerFeedbackResponse(string entityId, string customerId, int rating, DateTime reportedAt)
{
    public string EntityId => entityId;
    public string CustomerId => customerId;
    public int Rating => rating;
    public DateTime ReportedAt => reportedAt;
}
```
## Caso de uso: IGetCustomerFeedbackByRatingInputPort

```csharp
public interface IGetCustomerFeedbackByRatingInputPort
{
    Task<IEnumerable<CustomerFeedbackResponse>> HandleAsync(string id, int rating, DateTime? from, DateTime? end);
}
```

### Implementación del Caso de uso.

```csharp
internal class GetCustomerFeedbackByRatingHandler
    (IGetCustomerFeedbackByRatingRepository repository): IGetCustomerFeedbackByRatingInputPort
{
    public async Task<IEnumerable<CustomerFeedbackResponse>> HandleAsync(string id, int rating, DateTime? from, DateTime? end)
    {
        DateTime UtcFrom = from != null ? from.Value.Date
            : DateTime.UtcNow.Date.AddDays(-30);

        DateTime UtcEnd = end != null ? end.Value.Date.AddDays(1).AddTicks(-1)
            : DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

        return await repository.GetCustomerFeedbackByRatingAsync(id, rating , UtcFrom, UtcEnd);
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