using DigitalDoor.Reporting.Entities.Helpers;
using DigitalDoor.Reporting.Entities.Interfaces;
using DigitalDoor.Reporting.Entities.Models;
using DigitalDoor.Reporting.Entities.ValueObjects;
using DigitalDoor.Reporting.Entities.ViewModels;
using ISO9001.GenerateAuditReport.BusinessObjects.Interfaces;
using Org.BouncyCastle.Asn1.X509;

namespace ISO9001.GenerateAuditReport.Core.Handlers
{
    internal class GenerateAuditReportInputPort(
        IGenerateAuditReportRepository repository,
        IReportsOutputPort outputPort,
        IReportAsBytes reportBytes) : IGenerateAuditReportInputPort
    {
        public async ValueTask GenerateAuditReportAsync(string companyId, string entityId, DateTime? from, DateTime? end)
        {
            DateTime UtcFrom = from != null ? from.Value.Date
                : DateTime.UtcNow.Date.AddDays(-30);

            DateTime UtcEnd = end != null ? end.Value.Date.AddDays(1).AddTicks(-1)
                : DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

            var NonConformityResponses = await repository.GeAllNonConformitiessOrderByReportedAt(companyId, entityId, UtcFrom, UtcEnd);
            var IncidentReportResponses = await repository.GeAllIncidentReportsOrderByReportedAt(companyId, entityId, UtcFrom, UtcEnd);
            var CustomerFeedbackResponses = await repository.GetAllCustomerFeedbacksOrderByReportedAt(companyId, entityId, UtcFrom, UtcEnd);


            Setup reportSetUp = new(PageSize.A4, Orientation.Portrait);

            #region Header
            reportSetUp.Header = new Section(new Format(210, 40));

            reportSetUp.Header.AddColumn(new ColumnSetup
            {
                Format = new Format(210, 20)
                {
                    Position = new(20, 0),
                    FontDetails = new Font("Arial", new Shade(25, "Black"), new FontStyle(700)),
                    TextAlignment = TextAlignment.Center
                },
                DataColumn = new Item("HeaderText")
            });

            reportSetUp.Header.AddColumn(new ColumnSetup
            {
                Format = new Format(210, 20)
                {
                    Position = new(40, 20),
                    FontDetails = new Font("Arial", new Shade(14, "Black")),
                    TextAlignment = TextAlignment.Left
                },
                DataColumn = new Item("HeaderSubText")
            });
            #endregion

            #region BodyCreation
            reportSetUp.Body = new Section();
            reportSetUp.Body.Format = new Format
            {
                Dimension = new Dimension(210, 197),
                Position = new(0, 0),
            };
            #endregion

            int OffSetY = 0;

            #region CustomerFeedbacks
            reportSetUp.Body.Row = new Row { Dimension = new Dimension(200, 10) };
            reportSetUp.Body.AddColumn(new ColumnSetup
            {
                Format = new Format(200, 8)
                {
                    Position = new(OffSetY, 20),
                    FontDetails = new Font("Arial", new Shade(20, "Black"), new FontStyle(700)),
                    TextAlignment = TextAlignment.Left,

                },
                DataColumn = new Item("FeedbackTitle"),
            });

            OffSetY += 15;

            reportSetUp.Body.Row = new Row { Dimension = new Dimension(210, 9), };

            if (CustomerFeedbackResponses == null || !CustomerFeedbackResponses.Any())
            {
                reportSetUp.Body.AddColumn(new ColumnSetup
                {
                    Format = new Format(170, 7)
                    {
                        Position = new(10, (210 - 170) / 2),
                        FontDetails = new Font("Arial", new Shade(18)),
                        Background = "white",
                        TextAlignment = TextAlignment.Left,
                    },
                    DataColumn = new Item("NoFeedbackRecords"),
                });
                OffSetY += 9;
            }
            else
            {

                reportSetUp.Body.AddColumn(new ColumnSetup
                {
                    Format = new Format((int)(170 * 0.3), 7)
                    {
                        Position = new(OffSetY, (210 - 170) / 2),
                        Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                        FontDetails = new Font("Arial", new Shade(14, "White"), new FontStyle(700)),
                        Background = "gray",
                        TextAlignment = TextAlignment.Center,
                        Padding = new(1, 0, 2, 0)

                    },
                    DataColumn = new Item("FeedbackDateTitle"),
                });

                reportSetUp.Body.AddColumn(new ColumnSetup
                {
                    Format = new Format((int)(170 * 0.4), 7)
                    {
                        Position = new(OffSetY, (210 - 170) / 2 + (int)(170 * 0.3)),
                        Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                        FontDetails = new Font("Arial", new Shade(14, "White"), new FontStyle(700)),
                        Background = "gray",
                        TextAlignment = TextAlignment.Center,
                        Padding = new(1, 0, 2, 0)


                    },
                    DataColumn = new Item("FeedbackUserTitle"),
                });

                reportSetUp.Body.AddColumn(new ColumnSetup
                {
                    Format = new Format((int)(170 * 0.30), 7)
                    {
                        Position = new(OffSetY, (210 - 170) / 2 + (int)(170 * 0.3) + (int)(170 * 0.40)),
                        Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                        FontDetails = new Font("Arial", new Shade(14, "White"), new FontStyle(700)),
                        Background = "gray",
                        TextAlignment = TextAlignment.Center,
                        Padding = new(1, 0, 2, 0)


                    },
                    DataColumn = new Item("FeedbackGradeTitle"),
                });

                OffSetY += 9;

                int FeedbackRowHeight = 9;
                int TotalFeedbacks = CustomerFeedbackResponses.Count();
                int FeedbacksTableHeight = TotalFeedbacks * FeedbackRowHeight;

                reportSetUp.Body.Row = new Row { Dimension = new Dimension(210, 9), };

                reportSetUp.Body.AddColumn(new ColumnSetup
                {
                    Format = new Format((int)(170 * 0.3), 7)
                    {
                        Position = new(OffSetY, (210 - 170) / 2),
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
                    Format = new Format((int)(170 * 0.40), 7)
                    {
                        Position = new(OffSetY, (210 - 170) / 2 + (int)(170 * 0.3)),
                        Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                        FontDetails = new Font("Arial", new Shade(12)),
                        Background = "white",
                        TextAlignment = TextAlignment.Center,
                        Padding = new(2, 0, 0, 0)


                    },
                    DataColumn = new Item("FeedbackUserColumn"),
                });

                reportSetUp.Body.AddColumn(new ColumnSetup
                {
                    Format = new Format((int)(170 * 0.30), 7)
                    {
                        Position = new(OffSetY, (210 - 170) / 2 + (int)(170 * 0.3) + (int)(170 * 0.40)),
                        Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                        FontDetails = new Font("Arial", new Shade(12)),
                        Background = "white",
                        TextAlignment = TextAlignment.Center,
                        Padding = new(2, 0, 0, 0)

                    },
                    DataColumn = new Item("FeedbackGradeColumn"),
                });
                OffSetY += FeedbacksTableHeight;

            }
            #endregion

            #region NonConformity

            reportSetUp.Body.Row = new Row { Dimension = new Dimension(210, 10) };

            reportSetUp.Body.AddColumn(new ColumnSetup
            {
                Format = new Format(200, 8)
                {
                    Position = new(OffSetY, 20),
                    FontDetails = new Font("Arial", new Shade(20, "Black"), new FontStyle(700)),
                    Background = "white",
                    TextAlignment = TextAlignment.Left,
                    Padding = new(1, 0, 2, 0),

                },
                DataColumn = new Item("NonConformitiesTitle"),
            });

            OffSetY += 10;

            reportSetUp.Body.Row = new Row { Dimension = new Dimension(210, 9) };

            if (!NonConformityResponses.Any())
            {
                reportSetUp.Body.AddColumn(new ColumnSetup
                {
                    Format = new Format(170, 7)
                    {
                        Position = new(OffSetY, (210 - 170) / 2),
                        FontDetails = new Font("Arial", new Shade(18)),
                        Background = "white",
                        TextAlignment = TextAlignment.Left,
                        Padding = new(1, 0, 2, 0)
                    },
                    DataColumn = new Item("NoNonConformityRecords"),
                });
                OffSetY += 15;
            }
            else
            {
                OffSetY += 5;

                reportSetUp.Body.AddColumn(new ColumnSetup
                {
                    Format = new Format((int)(190 * 0.2), 7)
                    {
                        Position = new(OffSetY, ((210 - 190) / 2)),
                        Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                        FontDetails = new Font("Arial", new Shade(14, "White"), new FontStyle(700)),
                        Background = "gray",
                        TextAlignment = TextAlignment.Center,
                        Padding = new(1, 0, 2, 0)

                    },
                    DataColumn = new Item("NonConfDateTitle"),
                });
                reportSetUp.Body.AddColumn(new ColumnSetup
                {
                    Format = new Format((int)(190 * 0.2), 7)
                    {
                        Position = new(OffSetY, ((210 - 190) / 2) + (int)(190 * 0.2)),
                        Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                        FontDetails = new Font("Arial", new Shade(14, "White"), new FontStyle(700)),
                        Background = "gray",
                        TextAlignment = TextAlignment.Center,
                        Padding = new(1, 0, 2, 0)
                    },
                    DataColumn = new Item("NonConfIdTitle"),
                });
                reportSetUp.Body.AddColumn(new ColumnSetup
                {
                    Format = new Format((int)(190 * 0.20), 7)
                    {
                        Position = new(OffSetY, ((210 - 190) / 2) + (int)(190 * 0.4)),
                        Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                        FontDetails = new Font("Arial", new Shade(14, "White"), new FontStyle(700)),
                        Background = "gray",
                        TextAlignment = TextAlignment.Center,
                        Padding = new(1, 0, 2, 0)
                    },
                    DataColumn = new Item("NonConfProcessTitle"),
                });
                reportSetUp.Body.AddColumn(new ColumnSetup
                {
                    Format = new Format((int)(190 * 0.25), 7)
                    {
                        Position = new(OffSetY, ((210 - 190) / 2) + (int)(190 * 0.60)),
                        Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                        FontDetails = new Font("Arial", new Shade(14, "White"), new FontStyle(700)),
                        Background = "gray",
                        TextAlignment = TextAlignment.Center,
                        Padding = new(1, 0, 2, 0)
                    },
                    DataColumn = new Item("NonConfCauseTitle"),
                });
                reportSetUp.Body.AddColumn(new ColumnSetup
                {
                    Format = new Format((int)(190 * 0.15), 7)
                    {
                        Position = new(OffSetY, ((210 - 190) / 2) + (int)(190 * 0.85)),
                        Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                        FontDetails = new Font("Arial", new Shade(14, "White"), new FontStyle(700)),
                        Background = "gray",
                        TextAlignment = TextAlignment.Center,
                        Padding = new(1, 0, 2, 0)
                    },
                    DataColumn = new Item("NonConfStatusTitle"),
                });

                OffSetY += 9;

                int NonConformityRowHeight = 9;
                int TotalNonConformities = NonConformityResponses.Count();
                int TableNonConformityHeight = TotalNonConformities * NonConformityRowHeight;

                reportSetUp.Body.Row = new Row { Dimension = new Dimension(210, 9) };

                reportSetUp.Body.AddColumn(new ColumnSetup
                {
                    Format = new Format((int)(190 * 0.2), 7)
                    {
                        Position = new(OffSetY, ((210 - 190) / 2)),
                        Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                        FontDetails = new Font("Arial", new Shade(12, "black")),
                        Background = "white",
                        TextAlignment = TextAlignment.Center,
                        Padding = new(2, 0, 0, 0)

                    },
                    DataColumn = new Item("NonConfDateColumn"),
                });
                reportSetUp.Body.AddColumn(new ColumnSetup
                {
                    Format = new Format((int)(190 * 0.2), 7)
                    {
                        Position = new(OffSetY, ((210 - 190) / 2) + (int)(190 * 0.2)),
                        Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                        FontDetails = new Font("Arial", new Shade(10, "black")),
                        Background = "white",
                        TextAlignment = TextAlignment.Center,
                        Padding = new(1, 0, 1, 0)

                    },
                    DataColumn = new Item("NonConfIdColumn"),
                });
                reportSetUp.Body.AddColumn(new ColumnSetup
                {
                    Format = new Format((int)(190 * 0.20), 7)
                    {
                        Position = new(OffSetY, ((210 - 190) / 2) + (int)(190 * 0.4)),
                        Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                        FontDetails = new Font("Arial", new Shade(12, "black")),
                        Background = "white",
                        TextAlignment = TextAlignment.Center,
                        Padding = new(2, 0, 0, 0)

                    },
                    DataColumn = new Item("NonConfProcessColumn"),
                });
                reportSetUp.Body.AddColumn(new ColumnSetup
                {
                    Format = new Format((int)(190 * 0.25), 7)
                    {
                        Position = new(OffSetY, ((210 - 190) / 2) + (int)(190 * 0.60)),
                        Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                        FontDetails = new Font("Arial", new Shade(12, "black")),
                        Background = "white",
                        TextAlignment = TextAlignment.Center,
                        Padding = new(2, 0, 0, 0)

                    },
                    DataColumn = new Item("NonConfCauseColumn"),
                });
                reportSetUp.Body.AddColumn(new ColumnSetup
                {
                    Format = new Format((int)(190 * 0.15), 7)
                    {
                        Position = new(OffSetY, ((210 - 190) / 2) + (int)(190 * 0.85)),
                        Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                        FontDetails = new Font("Arial", new Shade(12, "black")),
                        Background = "white",
                        TextAlignment = TextAlignment.Center,
                        Padding = new(2, 0, 0, 0)

                    },
                    DataColumn = new Item("NonConfStatusColumn"),
                });
                OffSetY += TableNonConformityHeight;

            }
            #endregion

            #region IncidentReports

            reportSetUp.Body.Row = new Row { Dimension = new Dimension(210, 10) };
            reportSetUp.Body.AddColumn(new ColumnSetup
            {
                Format = new Format(200, 8)
                {
                    Position = new(OffSetY, 20),
                    FontDetails = new Font("Arial", new Shade(20, "Black"), new FontStyle(700)),
                    Background = "white",
                    TextAlignment = TextAlignment.Left,
                },
                DataColumn = new Item("IncidentReportsTitle"),
            });

            OffSetY += 10;

            reportSetUp.Body.Row = new Row { Dimension = new Dimension(210, 9) };

            if (!IncidentReportResponses.Any())
            {
                reportSetUp.Body.AddColumn(new ColumnSetup
                {
                    Format = new Format(170, 7)
                    {
                        Position = new(OffSetY, (210 - 170) / 2),
                        FontDetails = new Font("Arial", new Shade(18)),
                        Background = "white",
                        TextAlignment = TextAlignment.Left,
                        Padding = new(1, 0, 2, 0)
                    },
                    DataColumn = new Item("NoIncidentReportsRecords"),
                });
                OffSetY += 9;
            }
            else
            {
                OffSetY += 5;
                reportSetUp.Body.AddColumn(new ColumnSetup
                {
                    Format = new Format((int)(190 * 0.2), 7)
                    {
                        Position = new(OffSetY, ((210 - 190) / 2)),
                        Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                        FontDetails = new Font("Arial", new Shade(14, "White"), new FontStyle(700)),
                        Background = "gray",
                        TextAlignment = TextAlignment.Center,
                        Padding = new(1, 0, 2, 0)


                    },
                    DataColumn = new Item("IncidentReportsDateTitle"),
                });
                reportSetUp.Body.AddColumn(new ColumnSetup
                {
                    Format = new Format((int)(190 * 0.50), 7)
                    {
                        Position = new(OffSetY, ((210 - 190) / 2) + (int)(190 * 0.2)),
                        Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                        FontDetails = new Font("Arial", new Shade(14, "White"), new FontStyle(700)),
                        Background = "gray",
                        TextAlignment = TextAlignment.Center,
                        Padding = new(1, 0, 2, 0)

                    },
                    DataColumn = new Item("IncidentReportsDescTitle"),
                });
                reportSetUp.Body.AddColumn(new ColumnSetup
                {
                    Format = new Format((int)(190 * 0.15), 7)
                    {
                        Position = new(OffSetY, ((210 - 190) / 2) + (int)(190 * 0.7)),
                        Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                        FontDetails = new Font("Arial", new Shade(14, "White"), new FontStyle(700)),
                        Background = "gray",
                        TextAlignment = TextAlignment.Center,
                        Padding = new(1, 0, 2, 0)

                    },
                    DataColumn = new Item("IncidentReportsAffectedTitle"),
                });
                reportSetUp.Body.AddColumn(new ColumnSetup
                {
                    Format = new Format((int)(190 * 0.15), 7)
                    {
                        Position = new(OffSetY, ((210 - 190) / 2) + (int)(190 * 0.85)),
                        Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                        FontDetails = new Font("Arial", new Shade(14, "White"), new FontStyle(700)),
                        Background = "gray",
                        TextAlignment = TextAlignment.Center,
                        Padding = new(1, 0, 2, 0)

                    },
                    DataColumn = new Item("IncidentReportsSeverityTitle"),
                });

                OffSetY += 9;

                int IncidentReportRowHeight = 9;
                int TotalIncidentReports = IncidentReportResponses.Count();
                int TableIncidentReportHeight = TotalIncidentReports * IncidentReportRowHeight;

                reportSetUp.Body.Row = new Row { Dimension = new Dimension(210, 9) };

                reportSetUp.Body.AddColumn(new ColumnSetup
                {
                    Format = new Format((int)(190 * 0.2), 7)
                    {
                        Position = new(OffSetY, ((210 - 190) / 2)),
                        Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                        FontDetails = new Font("Arial", new Shade(12, "black")),
                        Background = "white",
                        TextAlignment = TextAlignment.Center,
                        Padding = new(2, 0, 0, 0)


                    },
                    DataColumn = new Item("IncidentReportsDateColumn"),
                });
                reportSetUp.Body.AddColumn(new ColumnSetup
                {
                    Format = new Format((int)(190 * 0.50), 7)
                    {
                        Position = new(OffSetY, ((210 - 190) / 2) + (int)(190 * 0.2)),
                        Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                        FontDetails = new Font("Arial", new Shade(12, "black")),
                        Background = "white",
                        TextAlignment = TextAlignment.Center,
                        Padding = new(2, 0, 0, 0)

                    },
                    DataColumn = new Item("IncidentReportsDescColumn"),
                });
                reportSetUp.Body.AddColumn(new ColumnSetup
                {
                    Format = new Format((int)(190 * 0.15), 7)
                    {
                        Position = new(OffSetY, ((210 - 190) / 2) + (int)(190 * 0.7)),
                        Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                        FontDetails = new Font("Arial", new Shade(12, "black")),
                        Background = "white",
                        TextAlignment = TextAlignment.Center,
                        Padding = new(2, 0, 0, 0)


                    },
                    DataColumn = new Item("IncidentReportsAffectedColumn"),
                });
                reportSetUp.Body.AddColumn(new ColumnSetup
                {
                    Format = new Format((int)(190 * 0.15), 7)
                    {
                        Position = new(OffSetY, ((210 - 190) / 2) + (int)(190 * 0.85)),
                        Borders = new Border(new Shade(1, "Black"), BorderStyle.solid),
                        FontDetails = new Font("Arial", new Shade(12, "black")),
                        Background = "white",
                        TextAlignment = TextAlignment.Center,
                        Padding = new(2, 0, 0, 0)

                    },
                    DataColumn = new Item("IncidentReportsSeverityColumn"),
                });

                OffSetY += TableIncidentReportHeight;

            }

            #endregion

            #region Footer
            reportSetUp.Footer = new Section(new Format(210, 50));
            reportSetUp.Footer.AddColumn(new ColumnSetup
            {
                Format = new Format(200, 10) { Position = new(5, 5) },
                DataColumn = new Item("FooterText")
            });
            #endregion

            var data = new List<ColumnData>();

            #region DataTitle
            data.Add(new ColumnData
            {
                Section = SectionType.Header,
                Column = new Item("HeaderText"),
                Value = $"Audit Report – Order: {entityId}",
                Row = 1
            });
            data.Add(new ColumnData
            {
                Section = SectionType.Header,
                Column = new Item("HeaderSubText"),
                Value = $"Fecha de creación: {UtcFrom:yyyy-MM-dd}\nEstatus actual: Delivered",
                Row = 2
            });
            #endregion

            #region FeedbackData
            data.Add(new ColumnData
            {
                Section = SectionType.Body,
                Column = new Item("FeedbackTitle"),
                Value = "1. Feedback de Cliente (Feedbacks)",
                Row = 1
            });

            if (CustomerFeedbackResponses == null || !CustomerFeedbackResponses.Any())
            {
                data.Add(new ColumnData
                {
                    Section = SectionType.Body,
                    Column = new Item("NoFeedbackRecords"),
                    Value = "No hay registros en estas fechas",
                    Row = 1
                });
            }
            else
            {
                data.Add(new ColumnData
                {
                    Section = SectionType.Body,
                    Column = new Item("FeedbackDateTitle"),
                    Value = "Fecha",
                    Row = 1
                });

                data.Add(new ColumnData
                {
                    Section = SectionType.Body,
                    Column = new Item("FeedbackUserTitle"),
                    Value = "UserId",
                    Row = 1
                });

                data.Add(new ColumnData
                {
                    Section = SectionType.Body,
                    Column = new Item("FeedbackGradeTitle"),
                    Value = "Rating",
                    Row = 1
                });


                int rowIndex = 1;
                foreach (var feedback in CustomerFeedbackResponses)
                {
                    data.Add(new ColumnData
                    {
                        Section = SectionType.Body,
                        Column = new Item("FeedbackDateColumn"),
                        Value = feedback.ReportedAt.ToString("yyyy-MM-dd"),
                        Row = rowIndex
                    });
                    data.Add(new ColumnData
                    {
                        Section = SectionType.Body,
                        Column = new Item("FeedbackUserColumn"),
                        Value = feedback.CustomerId ?? "",
                        Row = rowIndex
                    });
                    data.Add(new ColumnData
                    {
                        Section = SectionType.Body,
                        Column = new Item("FeedbackGradeColumn"),
                        Value = feedback.Rating.ToString() ?? "",
                        Row = rowIndex
                    });
                    rowIndex++;
                }
            }
            #endregion

            #region NonConformityData

            data.Add(new ColumnData
            {
                Section = SectionType.Body,
                Column = new Item("NonConformitiesTitle"),
                Value = "2. No Conformidades (NonConformities)",
                Row = 1
            });

            if (NonConformityResponses == null || !NonConformityResponses.Any())
            {
                data.Add(new ColumnData
                {
                    Section = SectionType.Body,
                    Column = new Item("NoNonConformityRecords"),
                    Value = "No hay registros en estas fechas",
                    Row = 1
                });
            }
            else
            {
                data.Add(new ColumnData
                {
                    Section = SectionType.Body,
                    Column = new Item("NonConfDateTitle"),
                    Value = "Fecha",
                    Row = 1
                });

                data.Add(new ColumnData
                {
                    Section = SectionType.Body,
                    Column = new Item("NonConfIdTitle"),
                    Value = "Id",
                    Row = 1
                });

                data.Add(new ColumnData
                {
                    Section = SectionType.Body,
                    Column = new Item("NonConfProcessTitle"),
                    Value = "Proccess",
                    Row = 1
                });


                data.Add(new ColumnData
                {
                    Section = SectionType.Body,
                    Column = new Item("NonConfCauseTitle"),
                    Value = "Cause",
                    Row = 1
                });

                data.Add(new ColumnData
                {
                    Section = SectionType.Body,
                    Column = new Item("NonConfStatusTitle"),
                    Value = "Status",
                    Row = 1
                });


                int rowIndex = 1;
                foreach (var nonConformity in NonConformityResponses)
                {
                    data.Add(new ColumnData
                    {
                        Section = SectionType.Body,
                        Column = new Item("NonConfDateColumn"),
                        Value = nonConformity.ReportedAt.ToString("yyyy-MM-dd"),
                        Row = rowIndex
                    });
                    data.Add(new ColumnData
                    {
                        Section = SectionType.Body,
                        Column = new Item("NonConfIdColumn"),
                        Value = nonConformity.Id.ToString() ?? "",
                        Row = rowIndex
                    });
                    data.Add(new ColumnData
                    {
                        Section = SectionType.Body,
                        Column = new Item("NonConfProcessColumn"),
                        Value = nonConformity.AffectedProcess.ToString() ?? "",
                        Row = rowIndex
                    });
                    data.Add(new ColumnData
                    {
                        Section = SectionType.Body,
                        Column = new Item("NonConfCauseColumn"),
                        Value = nonConformity.Cause.ToString() ?? "",
                        Row = rowIndex
                    });
                    data.Add(new ColumnData
                    {
                        Section = SectionType.Body,
                        Column = new Item("NonConfStatusColumn"),
                        Value = nonConformity.Status.ToString() ?? "",
                        Row = rowIndex
                    });
                    rowIndex++;
                }
            }
            #endregion

            #region IncidentReportsData

            data.Add(new ColumnData
            {
                Section = SectionType.Body,
                Column = new Item("IncidentReportsTitle"),
                Value = "3. Reportes de Incidencia (IncidentReports)",
                Row = 1
            });

            if (!IncidentReportResponses.Any())
            {
                data.Add(new ColumnData
                {
                    Section = SectionType.Body,
                    Column = new Item("NoIncidentReportsRecords"),
                    Value = "No hay registros en estas fechas",
                    Row = 1
                });
            }
            else
            {
                data.Add(new ColumnData
                {
                    Section = SectionType.Body,
                    Column = new Item("IncidentReportsDateTitle"),
                    Value = "Fecha",
                    Row = 1
                });

                data.Add(new ColumnData
                {
                    Section = SectionType.Body,
                    Column = new Item("IncidentReportsDescTitle"),
                    Value = "Descripcion",
                    Row = 1
                });

                data.Add(new ColumnData
                {
                    Section = SectionType.Body,
                    Column = new Item("IncidentReportsAffectedTitle"),
                    Value = "Process",
                    Row = 1
                });


                data.Add(new ColumnData
                {
                    Section = SectionType.Body,
                    Column = new Item("IncidentReportsSeverityTitle"),
                    Value = "Severity",
                    Row = 1
                });

                int rowIndex = 1;
                foreach (var incidentReport in IncidentReportResponses)
                {
                    data.Add(new ColumnData
                    {
                        Section = SectionType.Body,
                        Column = new Item("IncidentReportsDateColumn"),
                        Value = incidentReport.ReportedAt.ToString("yyyy-MM-dd"),
                        Row = rowIndex
                    });
                    data.Add(new ColumnData
                    {
                        Section = SectionType.Body,
                        Column = new Item("IncidentReportsDescColumn"),
                        Value = incidentReport.Description.ToString() ?? "",
                        Row = rowIndex
                    });
                    data.Add(new ColumnData
                    {
                        Section = SectionType.Body,
                        Column = new Item("IncidentReportsAffectedColumn"),
                        Value = incidentReport.AffectedProcess.ToString() ?? "",
                        Row = rowIndex
                    });
                    data.Add(new ColumnData
                    {
                        Section = SectionType.Body,
                        Column = new Item("IncidentReportsSeverityColumn"),
                        Value = incidentReport.Severity.ToString() ?? "",
                        Row = rowIndex
                    });
                    rowIndex++;
                }
            }
            #endregion


            await outputPort.Handle(reportSetUp, data);

            ReportViewModel reportModel = new ReportViewModel(reportSetUp, data);
            byte[] pdfBytes = await reportBytes.GenerateReport(reportModel);

            string folderPath = @"C:\Reports";
            Directory.CreateDirectory(folderPath);
            string fileName = $"PlainTextReport_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
            string fullPath = Path.Combine(folderPath, fileName);

            await File.WriteAllBytesAsync(fullPath, pdfBytes);
        }
    }
}
