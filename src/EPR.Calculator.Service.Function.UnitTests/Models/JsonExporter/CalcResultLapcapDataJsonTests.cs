using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.API.Data.DataModels;
using System.Text.Json;

namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    [TestClass]
    public class CalcResultLapcapDataJsonTests
    {
        [TestMethod]
        public void CalcResultLapcapDataJsonTests_CanCallFrom_WithValidData()
        {
            // Arrange
            var records = new List<CalcResultLapcapDataDetail>
            {
                new CalcResultLapcapDataDetail
                {
                    Name = "Paper",
                    EnglandCost = 50,
                    WalesCost = 60,
                    ScotlandCost = 70,
                    NorthernIrelandCost = 80,
                    TotalCost = 260
                },
                new CalcResultLapcapDataDetail
                {
                    Name = "Plastics",
                    EnglandCost = 100,
                    WalesCost = 200,
                    ScotlandCost = 300,
                    NorthernIrelandCost = 400,
                    TotalCost = 1000
                },
                new CalcResultLapcapDataDetail
                {
                    Name = CalcResultLapcapDataBuilder.Total,
                    EnglandCost = 1,
                    WalesCost = 2,
                    ScotlandCost = 3,
                    NorthernIrelandCost = 4,
                    TotalCost = 10
                }
            };

            var data = new CalcResultLapcapData
            {
                CalcResultLapcapDataDetails = records,
                CountryApportionment = new CountryApportionmentData
                {
                    England         = 0.47123m,
                    Wales           = 0.13123m,
                    Scotland        = 0.25123m,
                    NorthernIreland = 0.14631m
                }
            };

            // Act
            var result = CalcResultLapcapDataJson.From(data);


            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("LAPCAP Data", result.Name);
            Assert.IsNotNull(result.CalcResultLapcapDataDetails);
            var details = result.CalcResultLapcapDataDetails.ToList();
            Assert.IsTrue(details.Any(d => d.MaterialName == "Paper"));

            Assert.IsNotNull(result.CalcResultLapcapDataTotal);
            Assert.AreEqual("£1.00", result.CalcResultLapcapDataTotal!.TotalEnglandLaDisposalCost);
            Assert.AreEqual("£2.00", result.CalcResultLapcapDataTotal.TotalWalesLaDisposalCost);
            Assert.AreEqual("£3.00", result.CalcResultLapcapDataTotal.TotalScotlandLaDisposalCost);
            Assert.AreEqual("£4.00", result.CalcResultLapcapDataTotal.TotalNorthernIrelandLaDisposalCost);
            Assert.AreEqual("£10.00", result.CalcResultLapcapDataTotal.TotalLaDisposalCost);

            Assert.IsNotNull(result.OneCountryApportionmentPercentages);
            Assert.AreEqual("47.12300000%", result.OneCountryApportionmentPercentages!.EnglandApportionment);
            Assert.AreEqual("13.12300000%", result.OneCountryApportionmentPercentages.WalesApportionment);
            Assert.AreEqual("25.12300000%", result.OneCountryApportionmentPercentages.ScotlandApportionment);
            Assert.AreEqual("14.63100000%", result.OneCountryApportionmentPercentages.NorthernIrelandApportionment);
            Assert.AreEqual("100.00000000%", result.OneCountryApportionmentPercentages.TotalApportionment);

            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);
            var expectedJson = """
                {
                    "name": "LAPCAP Data",
                    "calcResultLapcapDataDetails": [
                        {
                            "materialName"                 : "Paper",
                            "englandLaDisposalCost"        : "£50.00",
                            "walesLaDisposalCost"          : "£60.00",
                            "scotlandLaDisposalCost"       : "£70.00",
                            "northernIrelandLaDisposalCost": "£80.00",
                            "oneLaDisposalCostTotal"       : "£260.00"
                        },
                        {
                            "materialName"                 : "Plastics",
                            "englandLaDisposalCost"        : "£100.00",
                            "walesLaDisposalCost"          : "£200.00",
                            "scotlandLaDisposalCost"       : "£300.00",
                            "northernIrelandLaDisposalCost": "£400.00",
                            "oneLaDisposalCostTotal"       : "£1,000.00"
                        }
                    ],
                    "calcResultLapcapDataTotal": {
                        "totalEnglandLaDisposalCost"        : "£1.00",
                        "totalWalesLaDisposalCost"          : "£2.00",
                        "totalScotlandLaDisposalCost"       : "£3.00",
                        "totalNorthernIrelandLaDisposalCost": "£4.00",
                        "totalLaDisposalCost"               : "£10.00"
                    },
                    "oneCountryApportionmentPercentages": {
                        "englandApportionment"        : "47.12300000%",
                        "walesApportionment"          : "13.12300000%",
                        "scotlandApportionment"       : "25.12300000%",
                        "northernIrelandApportionment": "14.63100000%",
                        "totalApportionment"          : "100.00000000%"
                    }
                }
                """;
            JsonTestUtils.AssertJson(expectedJson, json);
        }
    }
}
