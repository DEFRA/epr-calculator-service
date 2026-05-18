using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.API.Data.DataModels;
using System.Text.Json;
using EPR.Calculator.Service.Function.UnitTests.Builder;

namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    [TestClass]
    public class CalcResultLapcapDataJsonTests
    {
        [TestMethod]
        public void CalcResultLapcapDataJsonTests_CanCallFrom_WithValidData()
        {
            // Arrange
            var data = new CalcResultLapcapData
            {
                ByMaterial = new Dictionary<string, ByCountryValue>
                {
                    ["PC"] = new ByCountryValue { England = 50,  Wales = 60,  Scotland = 70,  NorthernIreland = 80,  Total = 260  },
                    ["PL"] = new ByCountryValue { England = 100, Wales = 200, Scotland = 300, NorthernIreland = 400, Total = 1000 }
                },
                Total = new ByCountryValue { England = 1, Wales = 2, Scotland = 3, NorthernIreland = 4, Total = 10 },
                CountryApportionment = new CountryApportionmentData
                {
                    England         = 0.47123m,
                    Wales           = 0.13123m,
                    Scotland        = 0.25123m,
                    NorthernIreland = 0.14631m
                }
            };
            var materials = TestDataHelper.GetMaterials();

            // Act
            var result = CalcResultLapcapDataJson.From(data, materials);


            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("LAPCAP Data", result.Name);
            Assert.IsNotNull(result.CalcResultLapcapDataDetails);
            var details = result.CalcResultLapcapDataDetails.ToList();
            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);
            var expectedJson = """
                {
                    "name": "LAPCAP Data",
                    "calcResultLapcapDataDetails": [
                        {
                            "materialName"                 : "Paper or card",
                            "englandLaDisposalCost"        : "£50.00",
                            "walesLaDisposalCost"          : "£60.00",
                            "scotlandLaDisposalCost"       : "£70.00",
                            "northernIrelandLaDisposalCost": "£80.00",
                            "oneLaDisposalCostTotal"       : "£260.00"
                        },
                        {
                            "materialName"                 : "Plastic",
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
