namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.OtherCosts
{
    using System.Text;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResultParameterOtherCostExporterTests
    {
        private CalcResultParameterOtherCostExporter exporter = new CalcResultParameterOtherCostExporter();

        [TestMethod]
        public void CanCallExportCommsCost()
        {
            // Arrange
            var otherCost = new CalcResultParameterOtherCost()
            {
                BadDebtProvision = new KeyValuePair<string, string>("key1", "6%"),
                Details =
                    [
                        new CalcResultParameterOtherCostDetail
                        {
                            Name = "4 LA Data Prep Charge",
                            OrderId = 1,
                            England = "£40.00",
                            EnglandValue = 40,
                            Wales = "£30.00",
                            WalesValue = 30,
                            Scotland = "£20.00",
                            ScotlandValue = 20,
                            NorthernIreland = "£10.00",
                            NorthernIrelandValue = 10,
                            Total = "£100.00",
                            TotalValue = 100,
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
                            England = "England",
                            EnglandValue = 100,
                            Wales = "Wales",
                            WalesValue = 50,
                            Scotland = "Scotland",
                            ScotlandValue = 80,
                            NorthernIreland = "Northern Ireland",
                            NorthernIrelandValue = 100,
                            Total = "Total",
                            TotalValue = 330,
                        },
                    ],
                SchemeSetupCost =
                    {
                        Name = "5 Scheme set up cost Yearly Cost",
                        OrderId = 1,
                        England = "£40.00",
                        EnglandValue = 40,
                        Wales = "£30.00",
                        WalesValue = 30,
                        Scotland = "£20.00",
                        ScotlandValue = 20,
                        NorthernIreland = "£10.00",
                        NorthernIrelandValue = 10,
                        Total = "£100.00",
                        TotalValue = 100,
                    },
            };
            var csvContent = new StringBuilder();
            ICalcResultParameterOtherCostExporter exporter = this.exporter;

            // Act
            this.exporter.Export(otherCost, csvContent);

            var result = csvContent.ToString();

            // Assert
            Assert.IsTrue(result.Contains(CommonConstants.England));
            Assert.IsTrue(result.Contains(CommonConstants.Wales));
            Assert.IsTrue(result.Contains(CommonConstants.Scotland));
            Assert.IsTrue(result.Contains(CommonConstants.NorthernIreland));
            Assert.IsTrue(result.Contains(CommonConstants.Total));
            Assert.IsTrue(result.Contains(CommonConstants.ParametersOther));
            Assert.IsTrue(result.Contains("6%"));
        }

        [TestMethod]
        public void CanCalLaDataPrepCosts()
        {
            // Arrange
            var otherCost = new CalcResultParameterOtherCost()
            {
                Name = "Parameters - Other",
                Details =
                    [
                        new CalcResultParameterOtherCostDetail
                        {
                            Name = "4 LA Data Prep Charge",
                            OrderId = 0,
                            England = CommonConstants.England,
                            Wales = CommonConstants.Wales,
                            Scotland = CommonConstants.Scotland,
                            NorthernIreland = CommonConstants.NorthernIreland,
                            Total = CommonConstants.Total,
                        },
                        new CalcResultParameterOtherCostDetail
                        {
                            Name = string.Empty,
                            OrderId = 1,
                            England = "£40.00",
                            Wales = "£30.00",
                            Scotland = "£20.00",
                            NorthernIreland = "£10.00",
                            Total = "£100.00",
                        },
                    ],
            };
            var csvContent = new StringBuilder();

            exporter.LaDataPrepCosts(otherCost, csvContent);
            var result = csvContent.ToString();

            // Assert
            Assert.IsTrue(result.Contains("England"));
            Assert.IsTrue(result.Contains("Wales"));
            Assert.IsTrue(result.Contains("Scotland"));
            Assert.IsTrue(result.Contains("Northern Ireland"));
            Assert.IsTrue(result.Contains("Total"));
            Assert.IsTrue(result.Contains("4 LA Data Prep Charge"));
            Assert.IsTrue(result.Contains("100"));
        }

        [TestMethod]
        public void CanCalSchemeSetupCost()
        {
            // Arrange
            var otherCost = new CalcResultParameterOtherCost()
            {
                Name = "Parameters - Other",
                SchemeSetupCost =
                    {
                        Name = "5 Scheme set up cost Yearly Cost",
                        OrderId = 1,
                        England = "£40.00",
                        Wales = "£30.00",
                        Scotland = "£20.00",
                        NorthernIreland = "£10.00",
                        Total = "£100.00",
                    },
            };
            var csvContent = new StringBuilder();

            // Assert
            exporter.SchemeSetupCost(otherCost, csvContent);
            var result = csvContent.ToString();
            Assert.IsTrue(result.Contains("£40"));
            Assert.IsTrue(result.Contains("£100"));
        }

        [TestMethod]
        public void CanCalMaterialityCost()
        {
            // Arrange
            var otherCost = new CalcResultParameterOtherCost()
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
            var otherCost = new CalcResultParameterOtherCost()
            {
                Name = "Parameters - Other",
                SaOperatingCost =
                    [
                        new CalcResultParameterOtherCostDetail
                        {
                            Name = "3 SA Operating Costs",
                            OrderId = 0,
                            England = CommonConstants.England,
                            Wales = CommonConstants.Wales,
                            Scotland = CommonConstants.Scotland,
                            NorthernIreland = CommonConstants.NorthernIreland,
                            Total = CommonConstants.Total,
                        },
                         new CalcResultParameterOtherCostDetail
                        {
                            Name = string.Empty,
                            OrderId = 1,
                            England = "100",
                            Wales = "50",
                            Scotland = "80",
                            NorthernIreland = "100",
                            Total = "330",
                        },
                    ],
            };
            var csvContent = new StringBuilder();
            exporter.SaOpertingCosts(otherCost, csvContent);
            var result = csvContent.ToString();

            // Assert
            Assert.IsTrue(result.Contains("England"));
            Assert.IsTrue(result.Contains("Wales"));
            Assert.IsTrue(result.Contains("Scotland"));
            Assert.IsTrue(result.Contains("Northern Ireland"));
            Assert.IsTrue(result.Contains("Total"));
            Assert.IsTrue(result.Contains("3 SA Operating Costs"));
            Assert.IsTrue(result.Contains("330"));
        }
    }
}