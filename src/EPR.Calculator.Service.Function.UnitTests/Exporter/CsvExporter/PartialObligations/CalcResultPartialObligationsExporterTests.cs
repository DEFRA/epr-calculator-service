
namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.PartialObligations
{
    using System.Text;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter.PartialObligations;

    [TestClass]
    public class CalcResultPartialObligationsExporterTests
    {
        private CalcResultPartialObligationsExporter exporter;

        public CalcResultPartialObligationsExporterTests()
        {
            exporter = new CalcResultPartialObligationsExporter();
        }

        [TestMethod]
        public void Export_ShouldIncludePartialObligations_WhenNotNull()
        {
            // Arrange
            var partialObligations = new CalcResultPartialObligations
            {
                TitleHeader = GetTitleHeader(),
                MaterialBreakdownHeaders = GetMaterialBreakdownHeaders(),
                ColumnHeaders = GetCoulmnHeaders(),
                PartialObligations = GetCalcResultPartialObligationsList(),
            };

            var csvContent = new StringBuilder();

            // Act
            exporter.Export(partialObligations, csvContent);
            var result = csvContent.ToString();

            // Assert
            Assert.IsTrue(result.Contains("101001"));
            Assert.IsTrue(result.Contains("Allied Packaging"));
            Assert.IsTrue(result.Contains(CalcResultPartialObligationHeaders.HouseholdPackagingWasteTonnage));
            Assert.IsTrue(result.Contains(CalcResultPartialObligationHeaders.PublicBinTonnage));
            Assert.IsTrue(result.Contains(CalcResultPartialObligationHeaders.TotalTonnage));
            Assert.IsTrue(result.Contains(CalcResultPartialObligationHeaders.SelfManagedConsumerWasteTonnage));
            Assert.IsTrue(result.Contains(CalcResultPartialObligationHeaders.NetTonnage));
            Assert.IsTrue(result.Contains(CalcResultPartialObligationHeaders.PartialHouseholdPackagingWasteTonnage));
            Assert.IsTrue(result.Contains(CalcResultPartialObligationHeaders.PartialPublicBinTonnage));
            Assert.IsTrue(result.Contains(CalcResultPartialObligationHeaders.PartialTotalTonnage));
            Assert.IsTrue(result.Contains(CalcResultPartialObligationHeaders.PartialSelfManagedConsumerWasteTonnage));
            Assert.IsTrue(result.Contains(CalcResultPartialObligationHeaders.PartialTotalTonnage));
        }

        [TestMethod]
        public void Export_PartialObligations_ShouldIncludeHeadersAndDisplayNone_WhenNoPartialObligation()
        {
            // Arrange
            var partialObligations = new CalcResultPartialObligations
            {
                TitleHeader = GetTitleHeader(),
                MaterialBreakdownHeaders = GetMaterialBreakdownHeaders(),
                ColumnHeaders = GetCoulmnHeaders(),
                PartialObligations = null,
            };

            var csvContent = new StringBuilder();

            // Act
            exporter.Export(partialObligations, csvContent);
            var result = csvContent.ToString();

            // Assert
            Assert.IsTrue(result.Contains("Partial Calculation"));
            Assert.IsTrue(result.Contains("Aluminium Breakdown"));
            Assert.IsTrue(result.Contains("Glass Breakdown"));
            Assert.IsTrue(result.Contains("Producer Id"));
            Assert.IsTrue(result.Contains("Subsidiary Id"));
            Assert.IsTrue(result.Contains("None"));
        }

        [TestMethod]
        public void Export_ShouldIncludeGlassColumns_WhenGlassMaterialPresent()
        {
            // Arrange
            var partialObligations = new CalcResultPartialObligations
            {
                TitleHeader = GetTitleHeader(),
                MaterialBreakdownHeaders = GetMaterialBreakdownHeaders(),
                ColumnHeaders = GetCoulmnHeaders(),
                PartialObligations = GetCalcResultPartialObligationsList(),
            };

            var csvContent = new StringBuilder();

            // Act
            exporter.Export(partialObligations, csvContent);
            var result = csvContent.ToString();

            // Assert
            Assert.IsTrue(result.Contains("Glass"));
            Assert.IsTrue(result.Contains("Household Drinks Containers Tonnage - Glass"));
            Assert.IsTrue(result.Contains("Partial Household Drinks Containers Tonnage - Glass"));
        }

        private List<CalcResultPartialObligation> GetCalcResultPartialObligationsList()
        {
            return new List<CalcResultPartialObligation>()
                {
                    new CalcResultPartialObligation
                    {
                        ProducerId = 101001,
                        ProducerName = "Allied Packaging",
                        DaysObligated = 183,
                        DaysInSubmissionYear = 366,
                        Level = "1",
                        JoiningDate = "15/07/2024",
                        ObligatedPercentage = "50.00%",
                        SubmissionYear = "2024",
                        SubsidiaryId = null,
                        PartialObligationTonnageByMaterial = new Dictionary<string, CalcResultPartialObligationTonnage>
                        {
                            {
                                "AL",
                                new CalcResultPartialObligationTonnage
                                {
                                    ReportedHouseholdPackagingWasteTonnage = 100,
                                    ReportedPublicBinTonnage = 20,
                                    TotalReportedTonnage = 120,
                                    ReportedSelfManagedConsumerWasteTonnage = 60,
                                    NetReportedTonnage = 180,
                                    PartialReportedHouseholdPackagingWasteTonnage = 50,
                                    PartialReportedPublicBinTonnage = 10,
                                    PartialTotalReportedTonnage = 60,
                                    PartialReportedSelfManagedConsumerWasteTonnage = 30,
                                    PartialNetReportedTonnage = 90,
                                }
                            },
                            {
                                "GL",
                                new CalcResultPartialObligationTonnage
                                {
                                    ReportedHouseholdPackagingWasteTonnage = 100,
                                    ReportedPublicBinTonnage = 20,
                                    TotalReportedTonnage = 120,
                                    ReportedSelfManagedConsumerWasteTonnage = 60,
                                    HouseholdDrinksContainersTonnageGlass = 70,
                                    NetReportedTonnage = 180,
                                    PartialReportedHouseholdPackagingWasteTonnage = 50,
                                    PartialReportedPublicBinTonnage = 10,
                                    PartialTotalReportedTonnage = 60,
                                    PartialReportedSelfManagedConsumerWasteTonnage = 30,
                                    PartialHouseholdDrinksContainersTonnageGlass = 35,
                                    PartialNetReportedTonnage = 90,
                                }
                            },
                        },
                    },
                };
        }

        private CalcResultPartialObligationHeader GetTitleHeader()
        {
            return new CalcResultPartialObligationHeader
            {
                Name = "Partial Calculation",
                ColumnIndex = 1,
            };
        }

        private IEnumerable<CalcResultPartialObligationHeader> GetMaterialBreakdownHeaders()
        {
            return new List<CalcResultPartialObligationHeader>
            {
                new CalcResultPartialObligationHeader
                {
                    Name = "Aluminium Breakdown",
                    ColumnIndex = 1,
                },
                new CalcResultPartialObligationHeader
                {
                    Name = "Glass Breakdown",
                    ColumnIndex = 2,
                },
            };
        }

        private IEnumerable<CalcResultPartialObligationHeader> GetCoulmnHeaders()
        {
            return new List<CalcResultPartialObligationHeader>()
            {
                new CalcResultPartialObligationHeader { Name = "Producer Id" },
                new CalcResultPartialObligationHeader { Name = "Subsidiary Id" },
                new CalcResultPartialObligationHeader { Name = "Producer / Subsidiary Name" },
                new CalcResultPartialObligationHeader { Name = "Trading Name" },
                new CalcResultPartialObligationHeader { Name = "Level" },
                new CalcResultPartialObligationHeader { Name = "Submission year" },
                new CalcResultPartialObligationHeader { Name = "Days in submission year" },
                new CalcResultPartialObligationHeader { Name = "Joining date" },
                new CalcResultPartialObligationHeader { Name = "Obligated days" },
                new CalcResultPartialObligationHeader { Name = "Obligated %" },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.HouseholdPackagingWasteTonnage },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PublicBinTonnage },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.HouseholdDrinksContainersTonnageGlass },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.TotalTonnage },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.SelfManagedConsumerWasteTonnage },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.NetTonnage },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialHouseholdPackagingWasteTonnage },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialPublicBinTonnage },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersTonnageGlass },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialTotalTonnage },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialSelfManagedConsumerWasteTonnage },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.PartialTotalTonnage },
            };
        }
    }
}