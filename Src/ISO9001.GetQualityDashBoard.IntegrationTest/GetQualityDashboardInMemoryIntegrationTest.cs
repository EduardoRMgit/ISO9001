using ISO9001.Database.InMemory;
using ISO9001.Database.InMemory.DataContexts.CustomerFeedbackDataContext;
using ISO9001.Database.InMemory.DataContexts.Entities;
using ISO9001.Database.InMemory.DataContexts.IncidentReportDataContext;
using ISO9001.Database.InMemory.DataContexts.NonConformityDataContext;
using ISO9001.Entities.Responses;
using ISO9001.GetQualityDashBoard.BusinessObjects.Interfaces;
using ISO9001.GetQualityDashBoard.IoC;
using ISO9001.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace ISO9001.GetQualityDashBoard.IntegrationTest
{
    public class GetQualityDashboardInMemoryIntegrationTest:IDisposable
    {
        readonly ServiceProvider Provider;
        readonly InMemoryCustomerFeedbackStore CustomerFeedbackMemoryStore;
        readonly InMemoryNonConformityStore NonConformityMemoryStore;
        readonly InMemoryIncidentReportStore IncidentReportStore;
        readonly IGetQualityDashBoardInputPort InputPort;
        private DateTime Now = new DateTime(2025, 09, 01, 0, 0, 0, DateTimeKind.Utc);


        public GetQualityDashboardInMemoryIntegrationTest()
        {
            var Services = new ServiceCollection();
            Services.AddGetQualityDashBoardServices();
            Services.AddISO9001Repositories();
            Services.AddDatabaseInMemory();

            Provider = Services.BuildServiceProvider();
            CustomerFeedbackMemoryStore = Provider.GetService<InMemoryCustomerFeedbackStore>();
            NonConformityMemoryStore = Provider.GetService<InMemoryNonConformityStore>();
            IncidentReportStore = Provider.GetService<InMemoryIncidentReportStore>();
            InputPort = Provider.GetRequiredService<IGetQualityDashBoardInputPort>();

            SeedDatabase();

        }

        void SeedDatabase()
        {            
            string CompanyId = "TestCompany";

            #region RegisterNonConformity

            var MasterDate1 = Now.AddMonths(-3);

            var NC1 = new NonConformity
            {
                Id = Guid.NewGuid(),
                EntityId = "nc-001",
                CompanyId = CompanyId,
                ReportedAt = MasterDate1,
                CreatedAt = MasterDate1,
                AffectedProcess = "Inspección",
                Cause = "Procedimiento mal definido",
                Status = "Open"
            };
            var NC1Detail1 = new NonConformityDetail
            {
                Id = ++NonConformityMemoryStore.NonConformityDetailsCurrentId,
                ReportedAt = MasterDate1,
                CreatedAt = MasterDate1,
                ReportedBy = "userB",
                Description = "Se detectó error en checklist",
                Status = "Open",
                NonConformityId = NC1.Id
            };
            var NC1Detail2 = new NonConformityDetail
            {
                Id = ++NonConformityMemoryStore.NonConformityDetailsCurrentId,
                ReportedAt = MasterDate1.AddDays(3),
                CreatedAt = MasterDate1.AddDays(3),
                ReportedBy = "userC",
                Description = "Supervisor confirmó inconsistencias",
                Status = "InProgress",
                NonConformityId = NC1.Id
            };

            var NC1Detail3 = new NonConformityDetail
            {
                Id = ++NonConformityMemoryStore.NonConformityDetailsCurrentId,
                ReportedAt = MasterDate1.AddDays(5),
                CreatedAt = MasterDate1.AddDays(5),
                ReportedBy = "userD",
                Description = "Se generó acción correctiva inicial",
                Status = "Closed",
                NonConformityId = NC1.Id
            };
            NC1.Status = NC1Detail3.Status;

            var MasterDate2 = Now.AddMonths(-2);


            var NC2 = new NonConformity
            {
                Id = Guid.NewGuid(),
                EntityId = "nc-002",
                CompanyId = CompanyId,
                ReportedAt = MasterDate2,
                CreatedAt = MasterDate2,
                AffectedProcess = "Producción",
                Cause = "Mantenimiento deficiente",
                Status = "Open"
            };

            var NC2Detail1 = new NonConformityDetail
            {
                Id = ++NonConformityMemoryStore.NonConformityDetailsCurrentId,
                ReportedAt = MasterDate2,
                CreatedAt = MasterDate2,
                ReportedBy = "userF",
                Description = "Operador reportó valores fuera de rango",
                Status = "Open",
                NonConformityId = NC2.Id
            };
            var NC2Detail2 = new NonConformityDetail
            {
                Id = ++NonConformityMemoryStore.NonConformityDetailsCurrentId,
                ReportedAt = MasterDate2.AddDays(5),
                CreatedAt = MasterDate2.AddDays(5),
                ReportedBy = "userG",
                Description = "Se revisaron registros de calibración",
                Status = "InProgress",
                NonConformityId = NC2.Id
            };
            var NC2Detail3 = new NonConformityDetail
            {
                Id = ++NonConformityMemoryStore.NonConformityDetailsCurrentId,
                ReportedAt = MasterDate2.AddDays(12),
                CreatedAt = MasterDate2.AddDays(12),
                ReportedBy = "userH",
                Description = "Se solicitó servicio externo de mantenimiento",
                Status = "Pending",
                NonConformityId = NC2.Id
            };
            NC2.Status = NC2Detail3.Status;

            var NC3Reported = Now.AddDays(-20);
            var NC3 = new NonConformity
            {
                Id = Guid.NewGuid(),
                EntityId = "nc-003",
                CompanyId = CompanyId,
                ReportedAt = NC3Reported,
                CreatedAt = NC3Reported,
                AffectedProcess = "Logística",
                Cause = "Falta de coordinación",
                Status = "Open"
            };

            var NC3Detail1 = new NonConformityDetail
            {
                Id = ++NonConformityMemoryStore.NonConformityDetailsCurrentId,
                ReportedAt = NC3Reported,
                CreatedAt = NC3Reported,
                ReportedBy = "userJ",
                Description = "Proveedor notificó retraso",
                Status = "Open",
                NonConformityId = NC3.Id
            };
            var NC3Detail2 = new NonConformityDetail
            {
                Id = ++NonConformityMemoryStore.NonConformityDetailsCurrentId,
                ReportedAt = NC3Reported.AddDays(10),
                CreatedAt = NC3Reported.AddDays(10),
                ReportedBy = "userK",
                Description = "Cliente fue informado del retraso",
                Status = "InProgress",
                NonConformityId = NC3.Id
            };
            var NC3Detail3 = new NonConformityDetail
            {
                Id = ++NonConformityMemoryStore.NonConformityDetailsCurrentId,
                ReportedAt = NC3Reported.AddDays(15),
                CreatedAt = NC3Reported.AddDays(15),
                ReportedBy = "userL",
                Description = "Se reprogramó entrega con nuevo plazo",
                Status = "Closed",
                NonConformityId = NC3.Id
            };
            NC3.Status = NC3Detail3.Status;

            NonConformityMemoryStore.NonConformities.AddRange([NC1, NC2, NC3]);
            NonConformityMemoryStore.NonConformityDetails.AddRange([
                NC1Detail1, NC1Detail2, NC1Detail3,
                NC2Detail1, NC2Detail2, NC2Detail3,
                NC3Detail1, NC3Detail2, NC3Detail3]);
            #endregion

            #region RegisterCustomerFeedback
            var RecentFeedback1 = new Database.InMemory.DataContexts.Entities.CustomerFeedback
            {
                Id = ++CustomerFeedbackMemoryStore.CurrentId,
                EntityId = "Entity1",
                CompanyId = CompanyId,
                CustomerId = "TestCustomer1",
                Rating = 1,
                Comments = "Feedback reciente 1",
                ReportedAt = Now,
                CreatedAt = Now
            };

            var RecentFeedback2 = new Database.InMemory.DataContexts.Entities.CustomerFeedback
            {
                Id = ++CustomerFeedbackMemoryStore.CurrentId,
                EntityId = "Entity2",
                CompanyId = CompanyId,
                CustomerId = "TestCustomer2",
                Rating = 2,
                Comments = "Feedback reciente 2",
                ReportedAt = Now.AddDays(-5),
                CreatedAt = Now.AddDays(-5)
            };

            var RecentFeedback3 = new Database.InMemory.DataContexts.Entities.CustomerFeedback
            {
                Id = ++CustomerFeedbackMemoryStore.CurrentId,
                EntityId = "Entity3",
                CompanyId = CompanyId,
                CustomerId = "TestCustomer1",
                Rating = 3,
                Comments = "Feedback reciente 3",
                ReportedAt = Now.AddDays(-10),
                CreatedAt = Now.AddDays(-10)
            };

            var RecentFeedback4 = new Database.InMemory.DataContexts.Entities.CustomerFeedback
            {
                Id = ++CustomerFeedbackMemoryStore.CurrentId,
                EntityId = "Entity4",
                CompanyId = CompanyId,
                CustomerId = "TestCustomer3",
                Rating = 4,
                Comments = "Feedback reciente 4",
                ReportedAt = Now.AddDays(-15),
                CreatedAt = Now.AddDays(-15)
            };

            var RecentFeedback5 = new Database.InMemory.DataContexts.Entities.CustomerFeedback
            {
                Id = ++CustomerFeedbackMemoryStore.CurrentId,
                EntityId = "Entity5",
                CompanyId = CompanyId,
                CustomerId = "TestCustomer2",
                Rating = 5,
                Comments = "Feedback reciente 5",
                ReportedAt = Now.AddDays(-20),
                CreatedAt = Now.AddDays(-20)
            };

            var OldFeedback1 = new Database.InMemory.DataContexts.Entities.CustomerFeedback
            {
                Id = ++CustomerFeedbackMemoryStore.CurrentId,
                EntityId = "Entity6",
                CompanyId = CompanyId,
                CustomerId = "TestCustomer1",
                Rating = 1,
                Comments = "Feedback antiguo 1",
                ReportedAt = Now.AddDays(-35),
                CreatedAt = Now.AddDays(-35)
            };

            var OldFeedback2 = new Database.InMemory.DataContexts.Entities.CustomerFeedback
            {
                Id = ++CustomerFeedbackMemoryStore.CurrentId,
                EntityId = "Entity7",
                CompanyId = CompanyId,
                CustomerId = "TestCustomer3",
                Rating = 2,
                Comments = "Feedback antiguo 2",
                ReportedAt = Now.AddDays(-45),
                CreatedAt = Now.AddDays(-45)
            };

            var OldFeedback3 = new Database.InMemory.DataContexts.Entities.CustomerFeedback
            {
                Id = ++CustomerFeedbackMemoryStore.CurrentId,
                EntityId = "Entity8",
                CompanyId = CompanyId,
                CustomerId = "TestCustomer2",
                Rating = 3,
                Comments = "Feedback antiguo 3",
                ReportedAt = Now.AddDays(-55),
                CreatedAt = Now.AddDays(-55)
            };
            CustomerFeedbackMemoryStore.CustomerFeedbacks.AddRange([
                RecentFeedback1, RecentFeedback2, RecentFeedback3, RecentFeedback4, RecentFeedback5,
                OldFeedback1, OldFeedback2, OldFeedback3]);

            #endregion

            #region RegisterIncidentReport
            var IncidentReport1 = new IncidentReport
            {
                Id = ++CustomerFeedbackMemoryStore.CurrentId,
                CompanyId = CompanyId,
                EntityId = "NC-001",
                ReportedAt = Now.AddMonths(-2),
                CreatedAt = Now.AddMonths(-2),
                UserId = "user-01",
                Description = "Falla en inspección inicial",
                AffectedProcess = "Proceso A",
                Severity = "Alta",
                Data = "{ \"Step\": 1 }"
            };

            var IncidentReport2 = new IncidentReport
            {
                Id = ++CustomerFeedbackMemoryStore.CurrentId,
                CompanyId = CompanyId,
                EntityId = "NC-001",
                ReportedAt = Now.AddMonths(-2),
                CreatedAt = Now.AddMonths(-2),
                UserId = "user-02",
                Description = "Corrección parcial aplicada",
                AffectedProcess = "Proceso A",
                Severity = "Media",
                Data = "{ \"Step\": 2 }"
            };

            var IncidentReport3 = new IncidentReport
            {
                Id = ++CustomerFeedbackMemoryStore.CurrentId,
                CompanyId = CompanyId,
                EntityId = "NC-002",
                ReportedAt = Now.AddMonths(-1).AddDays(-5),
                CreatedAt = Now.AddMonths(-1).AddDays(-5),
                UserId = "user-03",
                Description = "Error en proceso de validación",
                AffectedProcess = "Proceso B",
                Severity = "Alta",
                Data = "{ \"Check\": false }"
            };

            var IncidentReport4 = new IncidentReport
            {
                Id = ++CustomerFeedbackMemoryStore.CurrentId,
                CompanyId = CompanyId,
                EntityId = "NC-002",
                ReportedAt = Now.AddDays(-15),
                CreatedAt = Now.AddDays(-15),
                UserId = "user-01",
                Description = "Validación corregida",
                AffectedProcess = "Proceso B",
                Severity = "Baja",
                Data = "{ \"Check\": true }"
            };

            var IncidentReport5 = new IncidentReport
            {
                Id = ++CustomerFeedbackMemoryStore.CurrentId,
                CompanyId = CompanyId,
                EntityId = "NC-003",
                ReportedAt = Now.AddDays(-3),
                CreatedAt = Now.AddDays(-3),
                UserId = "user-04",
                Description = "Desviación en auditoría",
                AffectedProcess = "Proceso C",
                Severity = "Media",
                Data = "{ \"Audit\": \"Phase1\" }"
            };

            IncidentReportStore.IncidentReports.AddRange([
                IncidentReport1, IncidentReport2, IncidentReport3,
                IncidentReport4, IncidentReport5]);
            #endregion
        }

        [Fact]
        public async Task GetQualityDashboard_Of_The_Last_30_Days()
        {
            QualityDashboardResponse Result = await InputPort.HandleAsync("TestCompany", Now.AddDays(-30), Now);

            Assert.Equal(0, Result.OpenNonConformities);
            Assert.Equal(1, Result.ClosedNonConformities);
            Assert.Equal(TimeSpan.FromDays(7, 12), Result.AvarageResolutionDays);
            Assert.Equal(5, Result.TotalFeedbacks);
            Assert.Equal(3, Result.AvarageRating);
            Assert.Collection(Result.MonthlyKpis,
                item =>
                {
                    Assert.Equal("2025", item.Year);
                    Assert.Equal("09", item.Month);
                    Assert.Equal(0, item.NonConformities);
                    Assert.Equal(1, item.Feedbacks);
                },
                item =>
                {
                    Assert.Equal("2025", item.Year);
                    Assert.Equal("08", item.Month);
                    Assert.Equal(1, item.NonConformities);
                    Assert.Equal(4, item.Feedbacks);
                }
                );
        }

        [Fact]
        public async Task GetQualityDashboard_Of_The_Past_3_Months()
        {
            QualityDashboardResponse Result = await InputPort.HandleAsync("TestCompany", Now.AddMonths(-3), Now);


            Assert.Equal(1, Result.OpenNonConformities);
            Assert.Equal(2, Result.ClosedNonConformities);
            Assert.Equal(TimeSpan.FromDays(5, 8), Result.AvarageResolutionDays);
            Assert.Equal(8, Result.TotalFeedbacks);
            Assert.Equal(2.62, Result.AvarageRating);
            Assert.Equal(
                new Dictionary<string, int>
                {
                    {"NC-001", 2},
                    {"NC-002", 2},
                    {"NC-003", 1}
                },
                Result.IncidentsPerOrder);
            Assert.Equal(5, Result.TotalIncidentReports);
            Assert.Collection(Result.MonthlyKpis,
                item =>
                {
                    Assert.Equal("2025", item.Year);
                    Assert.Equal("09", item.Month);
                    Assert.Equal(0, item.NonConformities);
                    Assert.Equal(1, item.Feedbacks);
                },
                item =>
                {
                    Assert.Equal("2025", item.Year);
                    Assert.Equal("08", item.Month);
                    Assert.Equal(1, item.NonConformities);
                    Assert.Equal(4, item.Feedbacks);
                },
                item =>
                {
                    Assert.Equal("2025", item.Year);
                    Assert.Equal("07", item.Month);
                    Assert.Equal(1, item.NonConformities);
                    Assert.Equal(3, item.Feedbacks);
                },
                item =>
                {
                    Assert.Equal("2025", item.Year);
                    Assert.Equal("06", item.Month);
                    Assert.Equal(1, item.NonConformities);
                    Assert.Equal(0, item.Feedbacks);
                }
            );
        }

        public void Dispose()
        {
            Provider.Dispose();
        }
    }
}
