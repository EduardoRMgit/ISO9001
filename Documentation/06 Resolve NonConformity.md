# Registro y Resolución de la No Conformidad:
La forma en que se gestionan las no conformidades en un sistema conforme a ISO 9001 depende de los flujos de trabajo definidos en la organización, pero generalmente, hay dos tipos de roles involucrados en este proceso:

## Registro de la No Conformidad:

- Opción 1: El usuario final (cliente) puede ser quien registre la no conformidad, especialmente si se trata de un producto defectuoso o servicio no satisfactorio. Esto suele hacerse a través de un formulario en la aplicación web o en el sitio de soporte de la empresa. En este caso, el cliente reporta el incidente directamente a la empresa.
- Opción 2: El usuario de la empresa (ejemplo: agente de soporte o encargado de atención al cliente) podría ser quien registre la no conformidad después de recibir la llamada, correo o cualquier otra comunicación del cliente. Este es un flujo común, ya que las empresas prefieren manejar el reporte de no conformidades desde un punto centralizado para darles seguimiento adecuado.

En ambos casos, el flujo sería similar, pero el punto de inicio (quién lo registra) cambia.

Ejemplo de flujo:

- El usuario final podría ingresar una queja en un formulario dentro de la aplicación o en un portal.
- El usuario de la empresa (por ejemplo, el agente de soporte o el responsable de calidad) recibe la queja y, si es necesario, completa un formulario similar para registrar la no conformidad internamente. El formulario puede contener más detalles sobre el producto/servicio afectado.

## Resolución de la No Conformidad:

- Opción 1: El usuario de la empresa (por ejemplo, un miembro del equipo de soporte o del departamento de calidad) es quien gestionará la resolución, siguiendo los procedimientos internos para investigar la causa, implementar las acciones correctivas y cerrar la no conformidad.
- Opción 2: El usuario final (en algunos casos específicos) podría ser informado de la resolución, pero típicamente no es quien hace la resolución directamente. Sin embargo, el cliente puede recibir notificaciones de seguimiento o resolución a través de correo electrónico o un portal de usuario.

Ejemplo de flujo:

- El usuario de la empresa (por ejemplo, el encargado de calidad o soporte técnico) investiga el problema, aplica las correcciones necesarias y documenta la resolución. Posteriormente, la no conformidad se marca como cerrada en el sistema.
- El usuario final (cliente) puede recibir una notificación o correo confirmando que la no conformidad ha sido resuelta.

# En términos de la aplicación:
Para el Registro de la No Conformidad:

- Si el cliente es quien registra la no conformidad, podrías tener un formulario o sección en la aplicación donde el cliente puede reportar problemas o incidentes relacionados con productos o servicios.
- Si el empleado de la empresa es quien lo hace, se podría incluir un panel interno donde el empleado pueda ingresar detalles de la queja o no conformidad tras recibirla por teléfono o correo electrónico.

## Para la Resolución de la No Conformidad:

El proceso de resolución generalmente será gestionado por el usuario de la empresa. Esto puede ser realizado en un panel de administración o interfaz interna para que el empleado registre las acciones correctivas y cierre la no conformidad. Sin embargo, una vez resuelta, el cliente debe recibir una notificación o confirmación de resolución (por correo electrónico o en la interfaz de usuario si es una aplicación web o móvil).

### Ejemplo de Flujo en tu Aplicación:

#### Formulario de Registro de No Conformidad para el Usuario Final (Cliente):
- Un cliente accede a una sección en la aplicación de "Soporte" o "Reportar Incidencias".
- Completa un formulario con detalles del incidente (producto defectuoso, error en el servicio, etc.).
- La no conformidad se crea en el sistema y se asigna para su investigación.

#### Panel de Gestión para el Usuario de la Empresa (Empleado):
- Un empleado (como un agente de soporte) accede a un panel de "Gestión de Incidencias" o "Gestión de No Conformidades".
- Ve una lista de incidentes reportados y puede seleccionar uno para investigar y gestionar.
- El empleado registra la resolución de la no conformidad (detalles de la acción correctiva, estado de resolución, etc.).
- La no conformidad se marca como resuelta y el cliente recibe una notificación de que el problema ha sido solucionado.

#### Notificación al Cliente:
Una vez resuelta la no conformidad, se envía una notificación al cliente confirmando que el problema ha sido resuelto, lo que puede hacerse a través de un correo electrónico, mensaje en la interfaz o ambos.

# Entidad: NonConformityDetail
Representa el seguimiento, comentario o acción tomada en relación a un caso de no conformidad. Puede haber varios NonConformityDetail por cada NonConformity. 
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

# Caso de uso: RegisterNonConformityDetail
El caso de uso RegisterNonConformityDetail es responsable dar seguimiento a la entidad maestra de no conformidad (NonConformity), actualizando su estado y registrando los detalles de la resolución.

## Parametros de Entrada.
- NonConformityCreateDetailRequest (obligatorio).

## Endpoint REST
Este endpoint permite registrar la entidad NonConformityDetail desde un cliente HTTP.

```csharp
public static class EndpointsMapper
{
    public static IEndpointRouteBuilder MapNonConformityEndpoints(
        this IEndpointRouteBuilder builder)
    {
        builder.MapPost(("{companyId}/" + "Detail").CreateEndpoint("NonConformityEndpoints"),
            async (
                string companyId,
                NonConformityCreateDetailRequest nonConformity, IRegisterNonConformityDetailInputPort inputPort) =>
            {
                NonConformityCreateDetailDto data = new NonConformityCreateDetailDto(
                    Guid.Parse(nonConformity.NonConformityId),
                    companyId,
                    nonConformity.ReportedAt,
                    nonConformity.ReportedBy,
                    nonConformity.Description,
                    nonConformity.Status);
                await inputPort.HandleAsync(data);
                return TypedResults.Created();
            });

        return builder;
    }
}
```


### DTO y Request

```csharp
public class NonConformityCreateDetailDto(Guid entityId, string companyId, DateTime reportedAt,
    string reportedBy, string description, string status)
{
    public Guid EntityId => entityId;
    public string CompanyId => companyId;
    public DateTime ReportedAt => reportedAt;
    public string ReportedBy => reportedBy;
    public string Description => description;
    public string Status => status;
}
```

```csharp
public class NonConformityCreateDetailRequest
{
    public DateTime ReportedAt { get; set; }
    public string NonConformityId { get; set; }
    public string ReportedBy { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
}
```

## Caso de uso: IRegisterNonConformityDetailInputPort

```csharp
public interface IRegisterNonConformityDetailInputPort
{
    Task HandleAsync(NonConformityCreateDetailDto nonConformityDetail);
}
```

### Implementación del Caso de uso.
Dentro del caso de uso, primero se válida si existe un NonConformity con el Id ingresada. En caso de que no exista se lanzará  una exepción, de lo contrario, se registrará el detalle en el sistema. Así mismo, se actualizará el estado del NonConformity maestro con el status del detalle ingresado y finalmente se guardán los cambios en el sistema.

Es importante resaltar que las definiciones de las interfaces "ICommandNonConformityDetailRepository", "ICommandNonConformityRepository" y "ICommandNonConformityRepository" al igual que sus implementaciones están definidas en la documentación "05 NonConformity.md".


```csharp
internal class RegisterNonConformityDetailHandler(
    ICommandNonConformityDetailRepository commandDetailRepository,
    ICommandNonConformityRepository commandMasterRepository,
    IQueryableNonConformityRepository queryRepository) : IRegisterNonConformityDetailInputPort
{
    public async Task HandleAsync(NonConformityCreateDetailDto nonConformityDetail)
    {
        bool NonConformityExists = await queryRepository.NonConformityExistsByGuidAsync(nonConformityDetail.EntityId);
        if (!NonConformityExists)
        {
            throw new InvalidOperationException("NonConformity doesn't exist");
        }
        else
        {
            await commandDetailRepository.RegisterNonConformityDetailAsync(nonConformityDetail);
            await commandMasterRepository.UpdateStatusNonConformityMasterAsync(nonConformityDetail.EntityId, nonConformityDetail.Status);
            await commandDetailRepository.SaveChangesAsync();
        }

    }
}
```

# Integración en Blazor WebAssembly (UI)
## ViewModel para Resolución
El ViewModel mantiene el estado de la resolución, que se enlazará con la interfaz de usuario de Blazor.

```csharp
public class ResolveNonConformityViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private Guid nonConformityId;
    private DateTime resolvedAt;
    private string resolutionDetails = string.Empty;
    private string status = string.Empty;

    public Guid NonConformityId
    {
        get => nonConformityId;
        set
        {
            if (nonConformityId != value)
            {
                nonConformityId = value;
                NotifyPropertyChanged(nameof(NonConformityId));
            }
        }
    }

    public DateTime ResolvedAt
    {
        get => resolvedAt;
        set
        {
            if (resolvedAt != value)
            {
                resolvedAt = value;
                NotifyPropertyChanged(nameof(ResolvedAt));
            }
        }
    }

    public string ResolutionDetails
    {
        get => resolutionDetails;
        set
        {
            if (resolutionDetails != value)
            {
                resolutionDetails = value;
                NotifyPropertyChanged(nameof(ResolutionDetails));
            }
        }
    }

    public string Status
    {
        get => status;
        set
        {
            if (status != value)
            {
                status = value;
                NotifyPropertyChanged(nameof(Status));
            }
        }
    }

    private void NotifyPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
```

## Componente Blazor para Resolución de No Conformidades
Ahora vamos a crear un formulario en Blazor para resolver una no conformidad, donde un usuario puede añadir los detalles de resolución.

```razor
@page "/resolve-nonconformity/{NonConformityId}"
@inject IResolveNonConformityInputPort ResolveNonConformityInputPort
@inject NavigationManager Navigation

@code {
    [Parameter] public Guid NonConformityId { get; set; }
    private ResolveNonConformityViewModel viewModel = new ResolveNonConformityViewModel();
    private ResolveNonConformityPresenter? presenter;

    protected override async Task OnInitializedAsync()
    {
        presenter = new ResolveNonConformityPresenter(viewModel);
        viewModel.NonConformityId = NonConformityId;
    }

    private async Task HandleSubmit()
    {
        ResolveNonConformityRequest request = new ResolveNonConformityRequest
        {
            NonConformityId = viewModel.NonConformityId,
            ResolutionDetails = viewModel.ResolutionDetails,
            ResolvedBy = "Admin" // This could be dynamic based on the current user
        };

        await ResolveNonConformityInputPort.HandleAsync(request);
        Navigation.NavigateTo("/nonconformity-confirmation");
    }
}

<div class="container">
    <h3 class="title is-4">Resolve Non-Conformity</h3>

    <div class="box">
        <div class="field">
            <label class="label">Resolution Details</label>
            <div class="control">
                <textarea class="textarea" @bind="viewModel.ResolutionDetails" placeholder="Details of the resolution"></textarea>
            </div>
        </div>

        <button class="button is-primary" @onclick="HandleSubmit">Resolve</button>
    </div>

    @if (!string.IsNullOrEmpty(viewModel.Status))
    {
        <div class="notification is-success">
            @viewModel.Status
        </div>
    }
</div>
```

# Resumen de resolución de No Conformidad:
- Entidad NonConformity: Se amplió con detalles de resolución.
- Caso de Uso ResolveNonConformity: Actualiza el estado de la no conformidad y registra los detalles de la resolución.
- ViewModel ResolveNonConformityViewModel: Mantiene el estado de la resolución y se enlaza con la UI.
- Componente Blazor ResolveNonConformity.razor: Permite al usuario registrar los detalles de la resolución de una no conformidad.
