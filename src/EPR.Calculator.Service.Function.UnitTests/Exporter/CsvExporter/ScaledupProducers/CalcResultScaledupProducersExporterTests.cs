namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.ScaledupProducers
{
    using System.Text;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResultScaledupProducersExporterTests
    {
        private CalcResultScaledupProducersExporter exporter;

        public CalcResultScaledupProducersExporterTests()
        {
            exporter = new CalcResultScaledupProducersExporter();
        }

        [TestMethod]
        public void Export_ShouldIncludeScaledupProducers_WhenNotNull()
        {
            // Arrange
            var scaledupProducers = new CalcResultScaledupProducers
            {
                TitleHeader = GetTitleHeader(),
                MaterialBreakdownHeaders = GetMaterialBreakdownHeaders(),
                ColumnHeaders = GetCoulmnHeaders(),
                ScaledupProducers = GetCalcResultScaledupProducerList(),
            };

            var csvContent = new StringBuilder();

            // Act
            exporter.Export(scaledupProducers, csvContent);
            var result = csvContent.ToString();

            // Assert
            Assert.IsTrue(result.Contains("101001"));
            Assert.IsTrue(result.Contains("Allied Packaging"));
            Assert.IsTrue(result.Contains(CalcResultScaledupProducerHeaders.HouseholdPackagingWasteTonnage));
            Assert.IsTrue(result.Contains(CalcResultScaledupProducerHeaders.PublicBinTonnage));
            Assert.IsTrue(result.Contains(CalcResultScaledupProducerHeaders.TotalTonnage));
            Assert.IsTrue(result.Contains(CalcResultScaledupProducerHeaders.SelfManagedConsumerWasteTonnage));
            Assert.IsTrue(result.Contains(CalcResultScaledupProducerHeaders.NetTonnage));
            Assert.IsTrue(result.Contains(CalcResultScaledupProducerHeaders.ScaledupHouseholdPackagingWasteTonnage));
            Assert.IsTrue(result.Contains(CalcResultScaledupProducerHeaders.ScaledupPublicBinTonnage));
            Assert.IsTrue(result.Contains(CalcResultScaledupProducerHeaders.ScaledupTotalTonnage));
            Assert.IsTrue(result.Contains(CalcResultScaledupProducerHeaders.ScaledupSelfManagedConsumerWasteTonnage));
            Assert.IsTrue(result.Contains(CalcResultScaledupProducerHeaders.ScaledupTotalTonnage));            
        }

        [TestMethod]
        public void Export_ScaledUpProducer_ShouldIncludeHeadersAndDisplayNone_WhenNoScaledUpProducer()
        {
            // Arrange
            var scaledupProducers = new CalcResultScaledupProducers
            {
                TitleHeader = GetTitleHeader(),
                MaterialBreakdownHeaders = GetMaterialBreakdownHeaders(),
                ColumnHeaders = GetCoulmnHeaders(),
                ScaledupProducers = null!,
            };

            var csvContent = new StringBuilder();

            // Act
            exporter.Export(scaledupProducers, csvContent);
            var result = csvContent.ToString();

            // Assert
            Assert.IsTrue(result.Contains("Scaled-up Producers"));
            Assert.IsTrue(result.Contains("Each submission for the year"));
            Assert.IsTrue(result.Contains("Aluminium Breakdown"));
            Assert.IsTrue(result.Contains("Producer Id"));
            Assert.IsTrue(result.Contains("Subsidiary Id"));
            Assert.IsTrue(result.Contains("None"));
        }

        [TestMethod]
        public void Export_ShouldIncludeGlassColumns_WhenGlassMaterialPresent()
        {
            // Arrange
            var scaledupProducers = new CalcResultScaledupProducers
            {
                TitleHeader = GetTitleHeader(),
                MaterialBreakdownHeaders = GetMaterialBreakdownHeaders(),
                ColumnHeaders = GetCoulmnHeaders(),
                ScaledupProducers = GetCalcResultScaledupProducerList(),
            };

            var csvContent = new StringBuilder();

            // Act
            exporter.Export(scaledupProducers, csvContent);
            var result = csvContent.ToString();

            // Assert
            Assert.IsTrue(result.Contains("Glass"));
            Assert.IsTrue(result.Contains("Household Drinks Containers Tonnage - Glass"));
            Assert.IsTrue(result.Contains("Scaled-up Household Drinks Containers Tonnage - Glass"));
        }

        private List<CalcResultScaledupProducer> GetCalcResultScaledupProducerList()
        {
            var scaledupProducerList = new List<CalcResultScaledupProducer>();

            scaledupProducerList.AddRange([
                new CalcResultScaledupProducer()
                {
                    ProducerId = 101001,
                    SubsidiaryId = string.Empty,
                    ProducerName = "Allied Packaging",
                    Level = "1",
                    SubmissionPeriodCode = "2024-P2",
                    DaysInSubmissionPeriod = 91,
                    DaysInWholePeriod = 91,
                    ScaleupFactor = 2,
                    ScaledupProducerTonnageByMaterial = GetScaledupProducerTonnageByMaterial(),
                },
                new CalcResultScaledupProducer()
                {
                    ProducerId = 101001,
                    SubsidiaryId = string.Empty,
                    ProducerName = "Allied Packaging",
                    Level = "1",
                    SubmissionPeriodCode = "2024-P2",
                    DaysInSubmissionPeriod = 91,
                    DaysInWholePeriod = 91,
                    ScaleupFactor = 2,
                    ScaledupProducerTonnageByMaterial = GetScaledupProducerTonnageByMaterial(),
                    IsTotalRow = true,
                },
            ]);

            return scaledupProducerList;
        }

        private CalcResultScaledupProducerHeader GetTitleHeader()
        {
            return new CalcResultScaledupProducerHeader
            {
                Name = "Scaled-up Producers",
                ColumnIndex = 1,
            };
        }

        private IEnumerable<CalcResultScaledupProducerHeader> GetMaterialBreakdownHeaders()
        {
            return new List<CalcResultScaledupProducerHeader>
            {
                new CalcResultScaledupProducerHeader
                {
                    Name = "Each submission for the year",
                    ColumnIndex = 1,
                },
                new CalcResultScaledupProducerHeader
                {
                    Name = "Aluminium Breakdown",
                    ColumnIndex = 2,
                },
            };
        }

        private IEnumerable<CalcResultScaledupProducerHeader> GetCoulmnHeaders()
        {
            return new List<CalcResultScaledupProducerHeader>
            {
                new CalcResultScaledupProducerHeader { Name = "Producer Id" },
                new CalcResultScaledupProducerHeader { Name = "Subsidiary Id" },
                new CalcResultScaledupProducerHeader { Name = "Producer / Subsidiary Name" },
                new CalcResultScaledupProducerHeader { Name = "Trading Name" },
                new CalcResultScaledupProducerHeader { Name = "Level" },
                new CalcResultScaledupProducerHeader { Name = "Submission period code" },
                new CalcResultScaledupProducerHeader { Name = "Days in submission period" },
                new CalcResultScaledupProducerHeader { Name = "Days in whole period" },
                new CalcResultScaledupProducerHeader { Name = "Scale-up factor" },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.HouseholdPackagingWasteTonnage },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.PublicBinTonnage },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.HouseholdDrinksContainersTonnageGlass },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.TotalTonnage },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.SelfManagedConsumerWasteTonnage },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.NetTonnage },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupHouseholdPackagingWasteTonnage },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupPublicBinTonnage },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupHouseholdDrinksContainersTonnageGlass },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupTotalTonnage },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupSelfManagedConsumerWasteTonnage },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupTotalTonnage },
            };
        }

        private Dictionary<string, CalcResultScaledupProducerTonnage> GetScaledupProducerTonnageByMaterial()
        {
            return new Dictionary<string, CalcResultScaledupProducerTonnage>()
            {
                {
                    "AL",
                    new CalcResultScaledupProducerTonnage()
                    {
                        ReportedHouseholdPackagingWasteTonnage = 1000,
                        ReportedPublicBinTonnage = 100,
                        TotalReportedTonnage = 1100,
                        ReportedSelfManagedConsumerWasteTonnage = 500,
                        NetReportedTonnage = 1100,
                        ScaledupReportedHouseholdPackagingWasteTonnage = 2000,
                        ScaledupReportedPublicBinTonnage = 200,
                        ScaledupTotalReportedTonnage = 2200,
                        ScaledupReportedSelfManagedConsumerWasteTonnage = 1000,
                        ScaledupNetReportedTonnage = 2200,
                    }
                },
                {
                    "GL",
                    new CalcResultScaledupProducerTonnage()
                    {
                        ReportedHouseholdPackagingWasteTonnage = 1000,
                        ReportedPublicBinTonnage = 100,
                        HouseholdDrinksContainersTonnageGlass = 120,
                        TotalReportedTonnage = 1100,
                        ReportedSelfManagedConsumerWasteTonnage = 500,
                        NetReportedTonnage = 1100,
                        ScaledupReportedHouseholdPackagingWasteTonnage = 2000,
                        ScaledupReportedPublicBinTonnage = 200,
                        ScaledupHouseholdDrinksContainersTonnageGlass = 240,
                        ScaledupTotalReportedTonnage = 2200,
                        ScaledupReportedSelfManagedConsumerWasteTonnage = 1000,
                        ScaledupNetReportedTonnage = 2200,
                    }
                },
            };
        }
    }
}