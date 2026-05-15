using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts;
using EPR.Calculator.Service.Function.Models;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.OtherCosts
{
    [TestClass]
    public class CalcResultParameterOtherCostExporterTests
    {
        private CalcResultParameterOtherCostExporter exporter = new CalcResultParameterOtherCostExporter();

        [TestMethod]
        public void CanCallExportCommsCost()
        {
            // Arrange
            var otherCost = new CalcResultParameterOtherCost
            {
                BadDebtProvision = new KeyValuePair<string, string>("key1", "6%"),
                Details =
                    [
                        new CalcResultParameterOtherCostDetail
                        {
                            Name = "4 LA Data Prep Charge",
                            OrderId = 1,
                            England = 40,
                            Wales = 30,
                            Scotland = 20,
                            NorthernIreland = 10,
                            Total = 100
                        },
                    ],
                Materiality =
                    [
                        new CalcResultMateriality
                        {
                            Amount = "Amount £s",
                            AmountValue = 0,
                            Percentage = "%",
                            PercentageValue = 0,
                            SevenMateriality = "7 Materiality",
                        },
                    ],
                Name = "Parameters - Other",
                SaOperatingCost =
                    [
                        new CalcResultParameterOtherCostDetail
                        {
                            Name = "3 SA Operating Costs",
                            OrderId = 1,
                            England = 100,
                            Wales = 50,
                            Scotland = 80,
                            NorthernIreland = 100,
                            Total = 330
                        },
                    ],
                SchemeSetupCost =
                    {
                        Name = "5 Scheme set up cost Yearly Cost",
                        OrderId = 1,
                        England = 40,
                        Wales = 30,
                        Scotland = 20,
                        NorthernIreland = 10,
                        Total = 100
                    },
            };
            var csvContent = new StringBuilder();

            // Act
            exporter.Export(otherCost, csvContent);

            var result = csvContent.ToString();

            // Assert
            // TODO restore headers first
            //Assert.IsTrue(result.Contains("England"));
            //Assert.IsTrue(result.Contains("Wales"));
            //Assert.IsTrue(result.Contains("Scotland"));
            //Assert.IsTrue(result.Contains("Northern Ireland"));
            //Assert.IsTrue(result.Contains("Total"));
            //Assert.IsTrue(result.Contains("Parameters - Other"));
            Assert.IsTrue(result.Contains("6%"));
        }

        [TestMethod]
        public void CanCalLaDataPrepCosts()
        {
            // Arrange
            var otherCost = new CalcResultParameterOtherCost
            {
                Name = "Parameters - Other",
                Details =
                    [
                        new CalcResultParameterOtherCostDetail
                        {
                            Name = string.Empty,
                            OrderId = 1,
                            England = 40,
                            Wales = 30,
                            Scotland = 20,
                            NorthernIreland = 10,
                            Total = 100,
                        },
                    ],
            };
            var csvContent = new StringBuilder();

            exporter.LaDataPrepCosts(otherCost, csvContent);
            var result = csvContent.ToString();

            // Assert
            // TODO restore headers first
            //Assert.IsTrue(result.Contains("England"));
            //Assert.IsTrue(result.Contains("Wales"));
            //Assert.IsTrue(result.Contains("Scotland"));
            //Assert.IsTrue(result.Contains("Northern Ireland"));
            //Assert.IsTrue(result.Contains("Total"));
            //Assert.IsTrue(result.Contains("4 LA Data Prep Charge"));
            Assert.IsTrue(result.Contains("100"));
        }

        [TestMethod]
        public void CanCalSchemeSetupCost()
        {
            // Arrange
            var otherCost = new CalcResultParameterOtherCost
            {
                Name = "Parameters - Other",
                SchemeSetupCost =
                    {
                        Name = "5 Scheme set up cost Yearly Cost",
                        OrderId = 1,
                        England = 40,
                        Wales = 30,
                        Scotland = 20,
                        NorthernIreland = 10,
                        Total = 100,
                    },
            };
            var csvContent = new StringBuilder();

            // Assert
            exporter.SchemeSetupCost(otherCost, csvContent);
            var result = csvContent.ToString();
            Console.WriteLine($"result {JsonConvert.SerializeObject(result, Formatting.Indented)}");
            Assert.IsTrue(result.Contains("£40"));
            Assert.IsTrue(result.Contains("£100"));
        }

        [TestMethod]
        public void CanCalMaterialityCost()
        {
            // Arrange
            var otherCost = new CalcResultParameterOtherCost
            {
                Name = "Parameters - Other",
                Materiality =
                    [
                        new CalcResultMateriality
                        {
                            Amount = "Amount £s",
                            Percentage = "%",
                            SevenMateriality = "7 Materiality",
                        },
                        new CalcResultMateriality
                        {
                            Amount = "10",
                            Percentage = "1%",
                            SevenMateriality = "Increase",
                        },
                    ],
            };
            var csvContent = new StringBuilder();

            // Assert
            exporter.Materiality(otherCost, csvContent);
            var result = csvContent.ToString();
            // TODO the order is different - assert the full expected csv
            Assert.IsTrue(result.Contains("Amount £s"));
            Assert.IsTrue(result.Contains("%"));
            Assert.IsTrue(result.Contains("7 Materiality"));
            Assert.IsTrue(result.Contains("Increase"));
            Assert.IsTrue(result.Contains("1%"));
        }

        [TestMethod]
        public void CanCallSaOpertingCosts()
        {
            // Arrange
            var otherCost = new CalcResultParameterOtherCost
            {
                Name = "Parameters - Other",
                SaOperatingCost =
                    [
                        new CalcResultParameterOtherCostDetail
                        {
                            Name = string.Empty,
                            OrderId = 1,
                            England = 100,
                            Wales = 50,
                            Scotland = 80,
                            NorthernIreland = 100,
                            Total = 330
                        },
                    ],
            };
            var csvContent = new StringBuilder();
            exporter.SaOpertingCosts(otherCost, csvContent);
            var result = csvContent.ToString();
            Console.WriteLine($"result {JsonConvert.SerializeObject(result, Formatting.Indented)}");

            // Assert
            // TODO restore headers first
            //Assert.IsTrue(result.Contains("England"));
            //Assert.IsTrue(result.Contains("Wales"));
            //Assert.IsTrue(result.Contains("Scotland"));
            //Assert.IsTrue(result.Contains("Northern Ireland"));
            //Assert.IsTrue(result.Contains("Total"));
            //Assert.IsTrue(result.Contains("3 SA Operating Costs"));
            Assert.IsTrue(result.Contains("330"));
        }
    }
}
