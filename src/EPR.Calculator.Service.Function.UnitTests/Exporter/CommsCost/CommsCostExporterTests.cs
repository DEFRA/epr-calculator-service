namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CommsCost
{
    using System;
    using System.Text;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Exporter;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CommsCostExporterTests
    {
        private CommsCostExporter exporter = new CommsCostExporter();

        [TestMethod]
        public void ExportCommsCost_EmptyLists_ReturnsStringBuilderWithHeaders()
        {
            // Arrange
            var communicationCost = new CalcResultCommsCost
            {
                Name = "Parameters - Comms Costs",
                CalcResultCommsCostOnePlusFourApportionment = new List<CalcResultCommsCostOnePlusFourApportionment>(),
                CalcResultCommsCostCommsCostByMaterial = new List<CalcResultCommsCostCommsCostByMaterial>(),
                CommsCostByCountry = new List<CalcResultCommsCostOnePlusFourApportionment>(),
            };

            // Act
            var csvContent = new StringBuilder();
            this.exporter.Export(communicationCost, csvContent);
            var result = csvContent.ToString();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ToString().Contains("Parameters - Comms Costs"));
        }

        [TestMethod]
        public void ExportCommsCost_PopulatedLists_ReturnsStringBuilderWithData()
        {
            // Arrange
            var communicationCost = new CalcResultCommsCost
            {
                Name = "Parameters - Comms Costs",
                CalcResultCommsCostOnePlusFourApportionment = new List<CalcResultCommsCostOnePlusFourApportionment>
                    {
                        new CalcResultCommsCostOnePlusFourApportionment { Name = "1 + 4 Apportionment %s", England = "10", Wales = "20", Scotland = "30", NorthernIreland = "40", Total = "100" },
                    },
                CalcResultCommsCostCommsCostByMaterial = new List<CalcResultCommsCostCommsCostByMaterial>
                    {
                        new CalcResultCommsCostCommsCostByMaterial { Name = "2a Comms Costs - by Material", England = "10", Wales = "20", Scotland = "30", NorthernIreland = "40", Total = "100", ProducerReportedHouseholdPackagingWasteTonnage = "50", ReportedPublicBinTonnage = "60", HouseholdDrinksContainers = "70", LateReportingTonnage = "80", ProducerReportedHouseholdPlusLateReportingTonnage = "90", CommsCostByMaterialPricePerTonne = "100" },
                    },
                CommsCostByCountry = new List<CalcResultCommsCostOnePlusFourApportionment>
                    {
                        new CalcResultCommsCostOnePlusFourApportionment { Name = "2c Comms Costs - by Country", England = "10", Wales = "20", Scotland = "30", NorthernIreland = "40", Total = "100" },
                    },
            };

            // Act
            var csvContent = new StringBuilder();
            this.exporter.Export(communicationCost, csvContent);
            var result = csvContent.ToString();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ToString().Contains("1 + 4 Apportionment %s"));
            Assert.IsTrue(result.ToString().Contains("2a Comms Costs - by Material"));
            Assert.IsTrue(result.ToString().Contains("2c Comms Costs - by Country"));
        }
    }
}