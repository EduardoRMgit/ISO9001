# Generador de informe de auditoría
Lo primero es definir qué información debe contener el informe y de dónde se obtendrá esta información.

## Generador de Informe de Auditoría
El informe de auditoría debe contener una serie de eventos clave que se han registrado a lo largo del ciclo de vida de un pedido y la interacción con el sistema. Estos eventos son vitales para asegurar la trazabilidad, uno de los requisitos clave de ISO 9001.

Información que debe generar el informe:
- ID del Pedido: Identificador único del pedido que será auditado.
- Fecha de Creación del Pedido: Cuando el pedido fue creado en el sistema.
- Estado del Pedido: Estado actual del pedido (ej., "Pendiente", "En Proceso", "Completado").

Transacciones de Pago:

- Fecha de pago
- Monto pagado
- Estado del pago (ej., "Completado", "Fallido").
- Eventos de No Conformidad:
- Fecha en la que se reportó la no conformidad.
- Descripción de la no conformidad.
- Estado de la no conformidad (ej., "Pendiente", "Resuelta").
- Feedback del Cliente:
- Fecha del feedback.
- Calificación otorgada (por ejemplo, de 1 a 5).
- Comentarios adicionales.

Resoluciones de No Conformidad:

- Fecha de resolución de la no conformidad.
- Descripción de las acciones tomadas.
- Resultado de la resolución (ej., "Resuelto", "No resuelto").
- Historial de Cambios de Estado del Pedido: Registro de cualquier cambio en el estado del pedido (desde "Pendiente" hasta "Completado").
- Incidentes: Si el pedido tiene incidencias o problemas reportados, deben estar reflejados en el informe.
- Historial de Comunicaciones con el Cliente: Emails enviados o cualquier otra comunicación importante.

## Fuentes de Datos para el Informe:
- Pedidos (Orders): Los pedidos serán la base para generar el informe, ya que se registra el ID del pedido, su creación, su estado y su evolución.
- Transacciones de Pago (Payments): Los pagos están registrados y asociados a los pedidos, y deben incluirse para asegurar la trazabilidad de los cobros.
- No Conformidades (Non-Conformities): Los eventos de no conformidad deben ser rastreados a lo largo del ciclo del pedido. Pueden ser reportados en diferentes momentos y deben reflejarse en el informe.
- Feedback del Cliente (Customer Feedback): El feedback recibido por parte del cliente también es un dato clave para auditoría, ya que proporciona información sobre la calidad del servicio.
- Eventos de Cambio de Estado: Cada vez que el estado de un pedido cambie (por ejemplo, de "Pendiente" a "Completado"), esto debe registrarse como un evento de auditoría.
- Comunicaciones con el Cliente: Las comunicaciones, como los correos electrónicos o mensajes, deben quedar registradas con sus fechas y detalles.

## Generación del Informe de Auditoría - Flujo
- Solicitud de Informe: El sistema recibe una solicitud para generar un informe de auditoría de un pedido específico.
- Recopilación de Datos: El sistema recopila los datos de los repositorios de Pedidos, Pagos, No Conformidades, Feedbacks, Incidentes y Comunicaciones.
- Generación del Informe: Los datos recopilados se organizan en un informe estructurado, que puede ser generado en formatos como PDF o HTML.

## Implementación del Generador de Informe de Auditoría
A continuación, mostramos cómo podrías empezar a implementar el servicio para generar el informe de auditoría en C#.

## Repository: Interfaces

### IQueryableAuditReportRepository

```csharp
public interface IQueryableAuditReportRepository
{
    Task<IEnumerable<NonConformityMaterResponse>> GeAllNonConformitiessOrderByReportedAt(string companyId, string entityId, DateTime? from, DateTime? end);
    Task<IEnumerable<IncidentReportResponse>> GeAllIncidentReportsOrderByReportedAt(string companyId, string entityId, DateTime? from, DateTime? end);
    Task<IEnumerable<CustomerFeedbackResponse>> GetAllCustomerFeedbacksOrderByReportedAt(string companyId, string entityId, DateTime? from, DateTime? end);
}
```

## Implementación de los repositorios.

### QueryableAuditReportRepository
```csharp

    internal class QueryableAuditReportRepository(
        IQueryableCustomerFeedbackDataContext customerFeedbackDataContext,
        IQueryableNonConformityDataContext nonConformityDataContext,
        IQueryableIncidentReportDataContext incidentReportDataContext) : IQueryableAuditReportRepository
    {
        public async Task<IEnumerable<IncidentReportResponse>> GeAllIncidentReportsOrderByReportedAt(string companyId,
            string entityId, DateTime? from, DateTime? end)
        {
            var Query = incidentReportDataContext
                .IncidentReports.Where(
                IncidentReport => IncidentReport.CompanyId == companyId &&
                IncidentReport.EntityId == entityId &&
                IncidentReport.ReportedAt >= from &&
                IncidentReport.ReportedAt <= end)
                .OrderBy(IncidentReport => IncidentReport.ReportedAt);

            IEnumerable<IncidentReportReadModel> IncidentReports = await incidentReportDataContext.ToListAsync(Query);

            return IncidentReports.Select(IncidentReport => new IncidentReportResponse
            (
                IncidentReport.EntityId,
                IncidentReport.ReportedAt,
                IncidentReport.UserId,
                IncidentReport.Description,
                IncidentReport.AffectedProcess,
                IncidentReport.Severity,
                IncidentReport.Data
            ));
        }

        public async Task<IEnumerable<NonConformityMaterResponse>> GeAllNonConformitiessOrderByReportedAt(string companyId,
            string entityId, DateTime? from, DateTime? end)
        {
            var NonConformities = await nonConformityDataContext.ToListAsync(
                nonConformityDataContext.NonConformities
                    .Where(NonConformity => NonConformity.CompanyId == companyId &&
                           NonConformity.EntityId == entityId)
                    .OrderBy(NonConformity => NonConformity.ReportedAt));

            var MasterIds = NonConformities
                .Select(NonConformity => NonConformity.Id);

            var Details = await nonConformityDataContext.ToListAsync(
                nonConformityDataContext.NonConformityDetails
                    .Where(Detail => MasterIds.Contains(Detail.NonConformityId)).OrderBy(Detail => Detail.ReportedAt));

            return NonConformities.Select(NC => new NonConformityMaterResponse
            (
                NC.Id,
                NC.EntityId,
                NC.ReportedAt,
                NC.AffectedProcess,
                NC.Cause,
                NC.Status,
                Details.Count()
                ));
        }


        public async Task<IEnumerable<CustomerFeedbackResponse>> GetAllCustomerFeedbacksOrderByReportedAt(string companyId, string entityId,
            DateTime? from, DateTime? end)
        {
            var Query = customerFeedbackDataContext.CustomerFeedbacks
                .Where(CustomerFeedback => CustomerFeedback.CompanyId == companyId &&
                    CustomerFeedback.EntityId == entityId &&
                    CustomerFeedback.ReportedAt >= from &&
                    CustomerFeedback.ReportedAt <= end)
                .OrderBy(CustomerFeedback => CustomerFeedback.ReportedAt);

            var CustomerFeedbacks = await customerFeedbackDataContext.ToListAsync(Query);

            return CustomerFeedbacks.Select(CustomerFeedback => new CustomerFeedbackResponse
            (
                CustomerFeedback.EntityId,
                CustomerFeedback.CustomerId,
                CustomerFeedback.Rating,
                CustomerFeedback.ReportedAt
            )).ToList();
        }
    }
´´´


# Caso de uso: GenerateAuditReport
El caso de uso GenerateAuditReport es responsable de generar un reporte de auditoria en formato PDF con los eventos de auditoria de una entidad en un rango de fechas.

## Parametros de Entrada.
- companyId (obligatorio): Identificador de la empresa cuyos registros se desean consultar.
- entityId (obligatorio): Identificador de la entidad.
- from (opcional): Fecha de inicio del rango. Si no se especifica, se toma como valor predeterminado 30 días antes del día actual.
- end (opcional): Fecha de fin del rango. Si no se especifica, se toma como valor predeterminado el final del día actual.


## Endpoint REST
Este endpoint permite generar el reporte de auditoria en formato PDF.

```csharp
public static IEndpointRouteBuilder MapAuditReportEndpoints(
    this IEndpointRouteBuilder builder)
{
    builder.MapGet(
        "{companyId}/{entityId}/"
        .CreateEndpoint("AuditReportEndpoints"),
            async (
        string companyId,
        string entityId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? end,
        IGenerateAuditReportController controller) =>
            {
            byte[] Bytes = await controller.HandleAsync(companyId, entityId, from, end);
                return Results.File(Bytes, "application/pdf", "AuditReport.pdf");
            });
    return builder;
}
```

## Controlador de caso de uso: IGenerateAuditReportController
```csharp
public interface IGenerateAuditReportController
{
    Task<byte[]> HandleAsync(string companyId, string entityId, DateTime? from, DateTime? end);
}
```


## Caso de uso: IRegisterCustomerFeedbackInputPort

```csharp
internal interface IGenerateAuditReportInputPort
{
    ValueTask GenerateAuditReportAsync(string companyId, string entityId, DateTime? from, DateTime? end);
}
```

## Output port para caso de uso: IGenerateAuditReportOutputPort

```csharp
internal interface IGenerateAuditReportOutputPort
{
    public byte[] PdfBytes { get; }

    Task Handle(IEnumerable<NonConformityMaterResponse> nonConformityMaterResponses, IEnumerable<IncidentReportResponse> incidentReportResponses,
        IEnumerable<CustomerFeedbackResponse> customerFeedbackResponses, string entityId, DateTime from, DateTime end);
}
```

### Implementación del controlador.

```csharp
internal class GenerateAuditReportController(
    IGenerateAuditReportInputPort inputPort,
    IGenerateAuditReportOutputPort outputPort) : IGenerateAuditReportController
{
    public async Task<byte[]> HandleAsync(string companyId, string entityId, DateTime? from, DateTime? end)
    {
        await inputPort.GenerateAuditReportAsync(companyId, entityId, from, end);
        return outputPort.PdfBytes;

    }
}
```


### Implementación del caso de uso.
```csharp
internal class GenerateAuditReportHandler(
    IQueryableAuditReportRepository repository,
    IGenerateAuditReportOutputPort outputPort) : IGenerateAuditReportInputPort
{
    public async ValueTask GenerateAuditReportAsync(string companyId, string entityId, DateTime? from, DateTime? end)
    {
        DateTime UtcFrom = from != null ? from.Value.Date
            : DateTime.UtcNow.Date.AddDays(-30);

        DateTime UtcEnd = end != null ? end.Value.Date.AddDays(1).AddTicks(-1)
            : DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

        var NonConformities = await repository.GeAllNonConformitiessOrderByReportedAt(companyId, entityId, UtcFrom, UtcEnd);
        var Incidents = await repository.GeAllIncidentReportsOrderByReportedAt(companyId, entityId, UtcFrom, UtcEnd);
        var Feedbacks = await repository.GetAllCustomerFeedbacksOrderByReportedAt(companyId, entityId, UtcFrom, UtcEnd);
        await outputPort.Handle(NonConformities, Incidents, Feedbacks, entityId, UtcFrom, UtcEnd);

    }
}
```

### Implementación del presentador.
Para el presentador, se utilizaron los paquetes nugget DigitalDoor.Reporting v1.16.61 y DigitalDoor.Reporting.Presenters.PDF v1.16.61
```csharp
internal class GenerateAuditReportPresenter(
    IReportAsBytes reportBytes): IGenerateAuditReportOutputPort
{
    public byte[] PdfBytes { get; private set; }

    public async Task Handle(IEnumerable<NonConformityMaterResponse> nonConformityMaterResponses, 
        IEnumerable<IncidentReportResponse> incidentReportResponses, 
        IEnumerable<CustomerFeedbackResponse> customerFeedbackResponses, 
        string entityId, DateTime from, DateTime end)
    {
        Setup reportSetUp = new()
        {
            Page = new Format()
            {
                Orientation = Orientation.Portrait,
                Dimension = PageSize.A4,
                Background = "white",
                Padding = new(0, 0, 0, 0),
                Margin = new(0, 0, 0, 0),
                Position = new(0, 0, 0, 0)
            },
            Header = new Section(new Format(PageSize.A4.Width, 40)),
            Body = new Section(new Format(PageSize.A4.Width, 237)) { Row = new Row(new Dimension(PageSize.A4.Width, 9)) },
            Footer = new Section(new Format(PageSize.A4.Width, 20))
        };

        reportSetUp.Header.AddColumn(new ColumnSetup
        {
            Format = new Format(210, 20)
            {
                Position = new(15, 0),
                FontDetails = new Font("Arial", new Shade(25, "Black"), new FontStyle(700)),
                TextAlignment = TextAlignment.Center
            },
            DataColumn = new Item("HeaderText")
        });
        reportSetUp.Header.AddColumn(new ColumnSetup
        {
            Format = new Format(210, 20)
            {
                Position = new(35, 20),
                FontDetails = new Font("Arial", new Shade(14, "Black")),
                TextAlignment = TextAlignment.Left
            },
            DataColumn = new Item("HeaderSubText")
        });

        #region CustomerFeedbacks

        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format(210, 9)
            {
                FontDetails = new Font("Arial", new Shade(20, "Black"), new FontStyle(700)),
                TextAlignment = TextAlignment.Left,
                Padding = new(0, 0, 0, 20)

            },
            DataColumn = new Item("FeedbackTitle"),
        });

        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format(210, 9)
            {
                Margin = new(0, 0, 0, 20),

                FontDetails = new Font("Arial", new Shade(18)),
                Background = "white",
                TextAlignment = TextAlignment.Left,
                Padding = new(0, 0, 0, 20)

            },
            DataColumn = new Item("NoFeedbackRecords"),
        });


        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format((int)(170 * 0.3), 7)
            {
                Position = new(0, 20),
                Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                FontDetails = new Font("Arial", new Shade(14, "White"), new FontStyle(700)),
                Background = "gray",
                TextAlignment = TextAlignment.Center,
                Padding = new(1, 0, 2, 0),

            },
            DataColumn = new Item("FeedbackDateTitle"),
        });

        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format((int)(170 * 0.4), 7)
            {
                Position = new(0, 71),
                Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                FontDetails = new Font("Arial", new Shade(14, "White"), new FontStyle(700)),
                Background = "gray",
                TextAlignment = TextAlignment.Center,
                Padding = new(1, 0, 2, 0),

            },
            DataColumn = new Item("FeedbackUserIdTitle"),
        });

        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format((int)(170 * 0.3), 7)
            {
                Position = new(0, 139),
                Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                FontDetails = new Font("Arial", new Shade(14, "White"), new FontStyle(700)),
                Background = "gray",
                TextAlignment = TextAlignment.Center,
                Padding = new(1, 0, 2, 0),

            },
            DataColumn = new Item("FeedbackRatingTitle"),
        });


        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format((int)(170 * 0.3), 7)
            {
                Position = new(0, 20),
                Margin = new(0, 0, 0, 20),
                Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                FontDetails = new Font("Arial", new Shade(12)),
                Background = "white",
                TextAlignment = TextAlignment.Center,
                Padding = new(2, 0, 0, 0)


            },
            DataColumn = new Item("FeedbackDateColumn"),
        });

        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format((int)(170 * 0.4), 7)
            {
                Position = new(0, 71),
                Margin = new(0, 0, 0, 20),
                Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                FontDetails = new Font("Arial", new Shade(12)),
                Background = "white",
                TextAlignment = TextAlignment.Center,
                Padding = new(2, 0, 0, 0)


            },
            DataColumn = new Item("FeedbackUserIdColumn"),
        });

        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format((int)(170 * 0.3), 7)
            {
                Position = new(0, 139),
                Margin = new(0, 0, 0, 20),
                Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                FontDetails = new Font("Arial", new Shade(12)),
                Background = "white",
                TextAlignment = TextAlignment.Center,
                Padding = new(2, 0, 0, 0)
            },
            DataColumn = new Item("FeedbackRatingColumn"),
        });

        #endregion

        #region NonConformities

        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format(210, 9)
            {
                FontDetails = new Font("Arial", new Shade(20, "Black"), new FontStyle(700)),
                TextAlignment = TextAlignment.Left,
                Padding = new(0, 0, 0, 20)
            },
            DataColumn = new Item("NonConformityTitle"),
        });

        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format(210, 9)
            {
                FontDetails = new Font("Arial", new Shade(18)),
                Background = "white",
                TextAlignment = TextAlignment.Left,
                Padding = new(0, 0, 0, 20)

            },
            DataColumn = new Item("NoNonConformityRecords"),
        });


        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format((int)(190 * 0.20), 7)
            {
                Position = new(0, 10),
                Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                FontDetails = new Font("Arial", new Shade(14, "White"), new FontStyle(700)),
                Background = "gray",
                TextAlignment = TextAlignment.Center,
                Padding = new(1, 0, 2, 0)

            },
            DataColumn = new Item("NonConformityDateTitle"),
        });


        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format((int)(190 * 0.2), 7)
            {
                Position = new(0, 48),
                Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                FontDetails = new Font("Arial", new Shade(14, "White"), new FontStyle(700)),
                Background = "gray",
                TextAlignment = TextAlignment.Center,
                Padding = new(1, 0, 2, 0)

            },
            DataColumn = new Item("NonConformityIdTitle"),
        });


        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format((int)(190 * 0.2), 7)
            {
                Position = new(0, 86),
                Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                FontDetails = new Font("Arial", new Shade(14, "White"), new FontStyle(700)),
                Background = "gray",
                TextAlignment = TextAlignment.Center,
                Padding = new(1, 0, 2, 0)

            },
            DataColumn = new Item("NonConformityProcessTitle"),
        });


        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format((int)(190 * 0.25), 7)
            {
                Position = new(0, 124),
                Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                FontDetails = new Font("Arial", new Shade(14, "White"), new FontStyle(700)),
                Background = "gray",
                TextAlignment = TextAlignment.Center,
                Padding = new(1, 0, 2, 0)

            },
            DataColumn = new Item("NonConformityCauseTitle"),
        });


        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format((int)(190 * 0.15), 7)
            {
                Position = new(0, 171.5m),
                Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                FontDetails = new Font("Arial", new Shade(14, "White"), new FontStyle(700)),
                Background = "gray",
                TextAlignment = TextAlignment.Center,
                Padding = new(1, 0, 2, 0)

            },
            DataColumn = new Item("NonConformityStatusTitle"),
        });


        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format((int)(190 * 0.2), 7)
            {
                Position = new(0, 10),
                Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                FontDetails = new Font("Arial", new Shade(12, "black")),
                Background = "white",
                TextAlignment = TextAlignment.Center,
                Padding = new(2, 0, 0, 0)
            },
            DataColumn = new Item("NonConformityDateColumn"),
        });

        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format((int)(190 * 0.2), 7)
            {
                Position = new(0, 48),
                Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                FontDetails = new Font("Arial", new Shade(10, "black")),
                Background = "white",
                TextAlignment = TextAlignment.Center,
                Padding = new(1, 0, 1, 0)
            },
            DataColumn = new Item("NonConformityIdColumn"),
        });

        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format((int)(190 * 0.2), 7)
            {
                Position = new(0, 86),
                Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                FontDetails = new Font("Arial", new Shade(12, "black")),
                Background = "white",
                TextAlignment = TextAlignment.Center,
                Padding = new(2, 0, 0, 0)
            },
            DataColumn = new Item("NonConformityProcessColumn"),
        });

        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format((int)(190 * 0.25), 7)
            {
                Position = new(0, 124),
                Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                FontDetails = new Font("Arial", new Shade(12, "black")),
                Background = "white",
                TextAlignment = TextAlignment.Center,
                Padding = new(2, 0, 0, 0)
            },
            DataColumn = new Item("NonConformityCauseColumn"),
        });

        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format((int)(190 * 0.15), 7)
            {
                Position = new(0, 171.5m),
                Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                FontDetails = new Font("Arial", new Shade(12, "black")),
                Background = "white",
                TextAlignment = TextAlignment.Center,
                Padding = new(2, 0, 0, 0)
            },
            DataColumn = new Item("NonConformityStatusColumn"),
        });

        #endregion


        #region IncidentReports


        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format(210, 9)
            {
                FontDetails = new Font("Arial", new Shade(20, "Black"), new FontStyle(700)),
                TextAlignment = TextAlignment.Left,
                Padding = new(0, 0, 0, 20)
            },
            DataColumn = new Item("IncidentReportTitle"),
        });

        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format(210, 9)
            {
                FontDetails = new Font("Arial", new Shade(18)),
                Background = "white",
                TextAlignment = TextAlignment.Left,
                Padding = new(0, 0, 0, 20)

            },
            DataColumn = new Item("NoIncidentReportsRecords"),
        });

        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format((int)(190 * 0.2), 7)
            {
                Position = new(0, 10),
                Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                FontDetails = new Font("Arial", new Shade(14, "White"), new FontStyle(700)),
                Background = "gray",
                TextAlignment = TextAlignment.Center,
                Padding = new(1, 0, 2, 0),

            },
            DataColumn = new Item("IncidentReportDateTitle"),
        });


        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format((int)(190 * 0.5), 7)
            {
                Position = new(0, 48),
                Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                FontDetails = new Font("Arial", new Shade(14, "White"), new FontStyle(700)),
                Background = "gray",
                TextAlignment = TextAlignment.Center,
                Padding = new(1, 0, 2, 0),

            },
            DataColumn = new Item("IncidentReportDescriptionTitle"),
        });

        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format((int)(190 * 0.15), 7)
            {
                Position = new(0, 143),
                Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                FontDetails = new Font("Arial", new Shade(14, "White"), new FontStyle(700)),
                Background = "gray",
                TextAlignment = TextAlignment.Center,
                Padding = new(1, 0, 2, 0),

            },
            DataColumn = new Item("IncidentReportProcessTitle"),
        });

        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format((int)(190 * 0.15), 7)
            {
                Position = new(0, 171.5m),
                Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                FontDetails = new Font("Arial", new Shade(14, "White"), new FontStyle(700)),
                Background = "gray",
                TextAlignment = TextAlignment.Center,
                Padding = new(1, 0, 2, 0),

            },
            DataColumn = new Item("IncidentReportSeverityTitle"),
        });



        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format((int)(190 * 0.2), 7)
            {
                Position = new(0, 10),
                Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                FontDetails = new Font("Arial", new Shade(12)),
                Background = "white",
                TextAlignment = TextAlignment.Center,
                Padding = new(2, 0, 0, 0)
            },
            DataColumn = new Item("IncidentReportDateColumn"),
        });

        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format((int)(190 * 0.5), 7)
            {
                Position = new(0, 48),
                Margin = new(0, 0, 0, 20),
                Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                FontDetails = new Font("Arial", new Shade(12)),
                Background = "white",
                TextAlignment = TextAlignment.Center,
                Padding = new(2, 0, 0, 0)
            },
            DataColumn = new Item("IncidentReportDescriptionColumn"),
        });


        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format((int)(190 * 0.15), 7)
            {
                Position = new(0, 143),
                Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                FontDetails = new Font("Arial", new Shade(12)),
                Background = "white",
                TextAlignment = TextAlignment.Center,
                Padding = new(2, 0, 0, 0)
            },
            DataColumn = new Item("IncidentReportProcessColumn"),
        });

        reportSetUp.Body.AddColumn(new ColumnSetup
        {
            Format = new Format((int)(190 * 0.15), 7)
            {
                Position = new(0, 171.5m),
                Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                FontDetails = new Font("Arial", new Shade(12)),
                Background = "white",
                TextAlignment = TextAlignment.Center,
                Padding = new(2, 0, 0, 0)
            },
            DataColumn = new Item("IncidentReportProcessColumn"),
        });
        #endregion

        var LatestNonConformity = nonConformityMaterResponses
            .OrderByDescending(nc => nc.ReportedAt) 
            .FirstOrDefault();

        string LatestStatus = LatestNonConformity != null
            ? LatestNonConformity.Status
            : "Sin registros";




        int rowIndex = 1;

        var data = new List<ColumnData>
        {
            new ColumnData { Section = SectionType.Header, Column = new Item("HeaderText"), Value = $"Audit Report – Order: {entityId}" },
            new ColumnData { Section = SectionType.Header, Column = new Item("HeaderSubText"), Value = $"Fecha de creación: {DateTime.UtcNow:yyyy-MM-dd}\nEstatus actual: {LatestStatus}" },
        };

        data.Add(new ColumnData { Section = SectionType.Body, Column = new Item("FeedbackTitle"), Value = "1. Feedback de Cliente (Feedbacks)", Row = rowIndex++ });
        if (customerFeedbackResponses == null || !customerFeedbackResponses.Any())
        {
            data.Add(new ColumnData
            {
                Section = SectionType.Body,
                Column = new Item("NoFeedbackRecords"),
                Value = "No hay registros en estas fechas",
                Row = rowIndex++
            });
        }
        else
        {
            data.AddRange(new[]
            {
                new ColumnData { Section = SectionType.Body, Column = new Item("FeedbackDateTitle"), Value = "Fecha" , Row = rowIndex},
                new ColumnData { Section = SectionType.Body, Column = new Item("FeedbackUserIdTitle"), Value = "UserId" , Row = rowIndex},
                new ColumnData { Section = SectionType.Body, Column = new Item("FeedbackRatingTitle"), Value = "Rating" , Row = rowIndex},
            });
            rowIndex++;
            foreach (var feedback in customerFeedbackResponses)
            {
                data.Add(new ColumnData { Section = SectionType.Body, Column = new Item("FeedbackDateColumn"), Value = feedback.ReportedAt.ToString("yyyy-MM-dd") ?? "", Row = rowIndex });
                data.Add(new ColumnData { Section = SectionType.Body, Column = new Item("FeedbackUserIdColumn"), Value = feedback.CustomerId ?? "", Row = rowIndex });
                data.Add(new ColumnData { Section = SectionType.Body, Column = new Item("FeedbackRatingColumn"), Value = feedback.Rating, Row = rowIndex });
                rowIndex++;
            }
        }

        data.Add(new ColumnData { Section = SectionType.Body, Column = new Item("NonConformityTitle"), Value = "2. No Conformidades (NonConformities)", Row = rowIndex++ });
        if (nonConformityMaterResponses == null || !nonConformityMaterResponses.Any())
        {
            data.Add(new ColumnData
            {
                Section = SectionType.Body,
                Column = new Item("NoNonConformityRecords"),
                Value = "No hay registros en estas fechas",
                Row = rowIndex++
            });
        }
        else
        {
            data.AddRange(new[]
{
                new ColumnData { Section = SectionType.Body, Column = new Item("NonConformityDateTitle"), Value = "Fecha" , Row = rowIndex},
                new ColumnData { Section = SectionType.Body, Column = new Item("NonConformityIdTitle"), Value = "Id" , Row = rowIndex},
                new ColumnData { Section = SectionType.Body, Column = new Item("NonConformityProcessTitle"), Value = "Process" , Row = rowIndex},
                new ColumnData { Section = SectionType.Body, Column = new Item("NonConformityCauseTitle"), Value = "Cause" , Row = rowIndex},
                new ColumnData { Section = SectionType.Body, Column = new Item("NonConformityStatusTitle"), Value = "Status" , Row = rowIndex},
            });
            rowIndex++;
            foreach (var nonConformity in nonConformityMaterResponses)
            {
                data.Add(new ColumnData { Section = SectionType.Body, Column = new Item("NonConformityDateColumn"), Value = nonConformity.ReportedAt.ToString("yyyy-MM-dd HH:mm:ss"), Row = rowIndex });
                data.Add(new ColumnData { Section = SectionType.Body, Column = new Item("NonConformityIdColumn"), Value = nonConformity.Id.ToString(), Row = rowIndex });
                data.Add(new ColumnData { Section = SectionType.Body, Column = new Item("NonConformityProcessColumn"), Value = nonConformity.AffectedProcess, Row = rowIndex });
                data.Add(new ColumnData { Section = SectionType.Body, Column = new Item("NonConformityCauseColumn"), Value = nonConformity.Cause, Row = rowIndex });
                data.Add(new ColumnData { Section = SectionType.Body, Column = new Item("NonConformityStatusColumn"), Value = nonConformity.Status, Row = rowIndex });
                rowIndex++;
            }
        }


        data.Add(new ColumnData { Section = SectionType.Body, Column = new Item("IncidentReportTitle"), Value = "3. Reportes de Incidencia (IncidentReports)", Row = rowIndex++ });
        if (incidentReportResponses == null || !incidentReportResponses.Any())
        {
            data.Add(new ColumnData
            {
                Section = SectionType.Body,
                Column = new Item("NoIncidentReportsRecords"),
                Value = "No hay registros en estas fechas",
                Row = rowIndex++
            });
        }
        else
        {
            data.AddRange(new[]
            {
                new ColumnData { Section = SectionType.Body, Column = new Item("IncidentReportDateTitle"), Value = "Fecha" , Row = rowIndex},
                new ColumnData { Section = SectionType.Body, Column = new Item("IncidentReportDescriptionTitle"), Value = "Description" , Row = rowIndex},
                new ColumnData { Section = SectionType.Body, Column = new Item("IncidentReportProcessTitle"), Value = "Description" , Row = rowIndex},
                new ColumnData { Section = SectionType.Body, Column = new Item("IncidentReportSeverityTitle"), Value = "Description" , Row = rowIndex},

            });
            rowIndex++;
            foreach (var incidentReport in incidentReportResponses)
            {
                data.Add(new ColumnData { Section = SectionType.Body, Column = new Item("IncidentReportDateColumn"), Value = incidentReport.ReportedAt.ToString("yyyy-MM-dd") ?? "", Row = rowIndex });
                data.Add(new ColumnData { Section = SectionType.Body, Column = new Item("IncidentReportDescriptionColumn"), Value = incidentReport.Description, Row = rowIndex });
                data.Add(new ColumnData { Section = SectionType.Body, Column = new Item("IncidentReportProcessColumn"), Value = incidentReport.AffectedProcess, Row = rowIndex });
                data.Add(new ColumnData { Section = SectionType.Body, Column = new Item("IncidentReportSeverityColumn"), Value = incidentReport.Severity, Row = rowIndex });

                rowIndex++;
            }
        }

        ReportViewModel reportModel = new ReportViewModel(reportSetUp, data);
        byte[] pdfBytes = await reportBytes.GenerateReport(reportModel);
        PdfBytes = pdfBytes;

    }
}

```


## Conclusión
Este servicio de auditoría recopila los datos de los diferentes repositorios y genera un informe completo para ser utilizado por los auditores o el sistema de gestión de calidad. El informe contiene todos los eventos clave relacionados con un pedido y su trazabilidad en el sistema.

# Crear la Página Blazor para Mostrar el Informe
Primero, definimos el componente Blazor para mostrar el informe en formato HTML. Este componente tomará el AuditReport generado y lo mostrará en la interfaz de usuario.

## Componente Blazor AuditReportPage.razor
```razor
@page "/audit-report/{OrderId}"
@inject AuditReportService AuditReportService

@using YourNamespace.Models
@using System.Threading.Tasks

<h3>Audit Report for Order @OrderId</h3>

@if (auditReport == null)
{
    <p>Loading report...</p>
}
else
{
    <div class="audit-report">
        <h4>Order Information</h4>
        <table class="table is-bordered">
            <tr>
                <th>Order ID</th>
                <td>@auditReport.OrderId</td>
            </tr>
            <tr>
                <th>Order Creation Date</th>
                <td>@auditReport.OrderCreationDate</td>
            </tr>
            <tr>
                <th>Order Status</th>
                <td>@auditReport.OrderStatus</td>
            </tr>
        </table>

        <h4>Payment Transactions</h4>
        @if (auditReport.Payments.Any())
        {
            <table class="table is-bordered">
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>Amount</th>
                        <th>Status</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var payment in auditReport.Payments)
                    {
                        <tr>
                            <td>@payment.Date</td>
                            <td>@payment.Amount</td>
                            <td>@payment.Status</td>
                        </tr>
                    }
                </tbody>
            </table>
        }
        else
        {
            <p>No payment transactions found.</p>
        }

        <h4>Non-Conformities</h4>
        @if (auditReport.NonConformities.Any())
        {
            <table class="table is-bordered">
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>Description</th>
                        <th>Status</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var nonConformity in auditReport.NonConformities)
                    {
                        <tr>
                            <td>@nonConformity.Date</td>
                            <td>@nonConformity.Description</td>
                            <td>@nonConformity.Status</td>
                        </tr>
                    }
                </tbody>
            </table>
        }
        else
        {
            <p>No non-conformities reported.</p>
        }

        <h4>Customer Feedback</h4>
        @if (auditReport.Feedbacks.Any())
        {
            <table class="table is-bordered">
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>Rating</th>
                        <th>Comments</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var feedback in auditReport.Feedbacks)
                    {
                        <tr>
                            <td>@feedback.Date</td>
                            <td>@feedback.Rating</td>
                            <td>@feedback.Comments</td>
                        </tr>
                    }
                </tbody>
            </table>
        }
        else
        {
            <p>No customer feedback found.</p>
        }

        <h4>Incident History</h4>
        @if (auditReport.Incidents.Any())
        {
            <table class="table is-bordered">
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>Description</th>
                        <th>Status</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var incident in auditReport.Incidents)
                    {
                        <tr>
                            <td>@incident.Date</td>
                            <td>@incident.Description</td>
                            <td>@incident.Status</td>
                        </tr>
                    }
                </tbody>
            </table>
        }
        else
        {
            <p>No incidents reported.</p>
        }

        <h4>State Changes</h4>
        @if (auditReport.StateChanges.Any())
        {
            <ul>
                @foreach (var change in auditReport.StateChanges)
                {
                    <li>@change</li>
                }
            </ul>
        }
        else
        {
            <p>No state changes recorded.</p>
        }

        <h4>Communications</h4>
        @if (auditReport.Communications.Any())
        {
            <table class="table is-bordered">
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>Type</th>
                        <th>Details</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var communication in auditReport.Communications)
                    {
                        <tr>
                            <td>@communication.Date</td>
                            <td>@communication.Type</td>
                            <td>@communication.Details</td>
                        </tr>
                    }
                </tbody>
            </table>
        }
        else
        {
            <p>No communications recorded.</p>
        }
    </div>
}

@code {
    [Parameter]
    public string OrderId { get; set; }

    private AuditReport auditReport;

    protected override async Task OnInitializedAsync()
    {
        // Fetch the audit report data
        auditReport = await AuditReportService.GenerateReportAsync(OrderId);
    }
}
```
### Explicación del Código:
Inyección de Dependencias: Usamos la inyección de dependencias (AuditReportService) para obtener los datos del informe de auditoría. El servicio se encarga de recuperar todos los eventos y detalles relacionados con el pedido.

Parámetro de la URL (OrderId): El OrderId se obtiene desde la URL, y se pasa al componente como parámetro. Esto permite que cada vez que un usuario navegue a un OrderId específico, el informe correspondiente se cargue.

Carga de Datos del Informe: En el método OnInitializedAsync(), el componente llama al servicio AuditReportService para generar el informe de auditoría para el pedido indicado por OrderId.

Renderizado Condicional: 
- Si el informe está disponible (auditReport != null), se muestra la información en tablas HTML. Cada sección de datos (como pagos, retroalimentación, no conformidades, etc.) se presenta en una tabla separada.
- Si no hay datos (por ejemplo, no hay pagos o retroalimentación), se muestra un mensaje informando de ello.

Estilo de Bulma: Se utiliza Bulma para darle formato a las tablas y otros elementos. Las clases de Bulma como table, is-bordered, etc., se usan para dar estilo a las tablas de manera sencilla.

## Hacer que Blazor Use este Componente
Este componente será accesible en la ruta /audit-report/{OrderId}. Cuando accedas a esa URL, el componente tomará el OrderId de la URL y cargará los datos correspondientes.

Ejemplo de URL:
```bash
/audit-report/12345
```
Esto mostrará el informe de auditoría para el pedido con ID 12345.

### Conclusión
Este componente en Blazor permite generar y visualizar el informe de auditoría de manera estructurada. La información se organiza en secciones, y el formato HTML garantiza que sea fácilmente legible y accesible para los auditores o cualquier usuario autorizado que necesite revisar la trazabilidad del pedido.