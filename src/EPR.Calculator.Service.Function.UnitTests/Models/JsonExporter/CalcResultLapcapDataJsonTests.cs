namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    using System.Collections.Generic;
    using System.Linq;
    using EPR.Calculator.Service.Function.Builder.Lapcap;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Models.JsonExporter;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResultLapcapDataJsonTests
    {
        [TestMethod]
        public void CanCallFrom_WithValidData()
        {
            // Arrange
            var records = new List<CalcResultLapcapDataDetails>
            {
                new CalcResultLapcapDataDetails
                {
                    Name = "Paper",
                    EnglandDisposalCost = "£50",
                    WalesDisposalCost = "£60",
                    ScotlandDisposalCost = "£70",
                    NorthernIrelandDisposalCost = "£80",
                    TotalDisposalCost = "£260"
                },
                new CalcResultLapcapDataDetails
                {
                    Name = "Plastics",
                    EnglandDisposalCost = "£100",
                    WalesDisposalCost = "£200",
                    ScotlandDisposalCost = "£300",
                    NorthernIrelandDisposalCost = "£400",
                    TotalDisposalCost = "£1000"
                },
                new CalcResultLapcapDataDetails
                {
                    Name = CalcResultLapcapDataBuilder.Total,
                    EnglandDisposalCost = "£1",
                    WalesDisposalCost = "£2",
                    ScotlandDisposalCost = "£3",
                    NorthernIrelandDisposalCost = "£4",
                    TotalDisposalCost = "£10"
                },
                new CalcResultLapcapDataDetails
                {
                    Name = CalcResultLapcapDataBuilder.CountryApportionment,
                    EnglandDisposalCost = "80",
                    WalesDisposalCost = "70",
                    ScotlandDisposalCost = "60",
                    NorthernIrelandDisposalCost = "50",
                    TotalDisposalCost = "260"
                }
            };

            var data = new CalcResultLapcapData
            {
                Name = "Test Lapcap Data",
                CalcResultLapcapDataDetails = records,
            };

            // Act
            var result = CalcResultLapcapDataJson.From(data);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(data.Name, result.Name);
            Assert.IsNotNull(result.CalcResultLapcapDataDetails);
            var details = result.CalcResultLapcapDataDetails.ToList();
            Assert.IsTrue(details.Any(d => d.MaterialName == "Paper"));

            Assert.IsNotNull(result.CalcResultLapcapDataTotal);
            Assert.AreEqual("£1", result.CalcResultLapcapDataTotal!.TotalEnglandLaDisposalCost);
            Assert.AreEqual("£2", result.CalcResultLapcapDataTotal.TotalWalesLaDisposalCost);
            Assert.AreEqual("£3", result.CalcResultLapcapDataTotal.TotalScotlandLaDisposalCost);
            Assert.AreEqual("£4", result.CalcResultLapcapDataTotal.TotalNorthernIrelandLaDisposalCost);
            Assert.AreEqual("£10", result.CalcResultLapcapDataTotal.TotalLaDisposalCost);

            Assert.IsNotNull(result.OneCountryApportionmentPercentages);
            Assert.AreEqual("80", result.OneCountryApportionmentPercentages!.EnglandApportionment);
            Assert.AreEqual("70", result.OneCountryApportionmentPercentages.WalesApportionment);
            Assert.AreEqual("60", result.OneCountryApportionmentPercentages.ScotlandApportionment);
            Assert.AreEqual("50", result.OneCountryApportionmentPercentages.NorthernIrelandApportionment);
            Assert.AreEqual("260", result.OneCountryApportionmentPercentages.TotalApportionment);
        }
    }
}
