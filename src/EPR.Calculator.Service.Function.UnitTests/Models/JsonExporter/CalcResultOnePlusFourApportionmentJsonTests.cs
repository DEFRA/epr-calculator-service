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
                  "total"          : "£20.40"
                },
                "fourLADataPrepCharge": {
                  "england"        : "£0.10",
                  "wales"          : "£20.00",
                  "scotland"       : "£0.15",
                  "northernIreland": "£0.15",
                  "total"          : "£20.40"
                },
                "totalOfonePlusFour": {
                  "england"        : "£14.53",
                  "wales"          : "£20.00",
                  "scotland"       : "£0.15",
                  "northernIreland": "£0.15",
                  "total"          : "£34.83"
                },
                "onePlusFourApportionmentPercentages": {
                  "england"        : "40.00000000%",
                  "wales"          : "10.00000000%",
                  "scotland"       : "15.00000000%",
                  "northernIreland": "35.00000000%",
                  "total"          : "100.00000000%"
                }
                }
                """;

            JsonTestUtils.AssertJson(expectedJson, json);
        }
    }
}
