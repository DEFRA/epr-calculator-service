using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Function.UnitTests.Builder;
using System.Text.Json;

namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    [TestClass]
    public class CalcResultOnePlusFourApportionmentJsonTests
    {
        [TestMethod]
        public void OnePlusFourApportionmentJson_From()
        {
            var onePlusFourApportionment = TestDataHelper.GetCalcResultOnePlusFourApportionment();
            var result = CalcResultOnePlusFourApportionmentJson.From(onePlusFourApportionment);

            Assert.IsNotNull(result);

            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);

            var expectedJson = """
                {
                "oneFeeForLADisposalCosts": {
                    "england"        : "£0.10",
                    "wales"          : "£20.00",
                    "scotland"       : "£0.15",
                    "northernIreland": "£0.15",
                    "total"          : "£0.10"
                },
                "fourLADataPrepCharge": {
                    "england"        : "£0.10",
                    "wales"          : "£20.00",
                    "scotland"       : "£0.15",
                    "northernIreland": "£0.15",
                    "total"          : "£0.10"
                },
                "totalOfonePlusFour": {
                    "england"        : "£14.53",
                    "wales"          : "£20.00",
                    "scotland"       : "£0.15",
                    "northernIreland": "£0.15",
                    "total"          : "£0.10"
                },
                "onePlusFourApportionmentPercentages": {
                    "england"        : "40%",
                    "wales"          : "10%",
                    "scotland"       : "15%",
                    "northernIreland": "35%",
                    "total"          : "100%"
                }
                }
                """;

            JsonTestUtils.AssertJson(expectedJson, json);
        }
    }
}
