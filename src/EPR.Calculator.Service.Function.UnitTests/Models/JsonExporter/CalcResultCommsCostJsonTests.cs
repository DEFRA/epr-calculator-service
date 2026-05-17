using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Function.UnitTests.Builder;
using System.Text.Json;

namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    [TestClass]
    public class CalcResultCommsCostJsonTests {

        [TestMethod]
        public void CalcResultCommsCost_From()
        {
            var data = TestDataHelper.GetCalcResultCommsCostReportDetail();

            var result = CalcResultCommsCostJson.From(data);
            Console.WriteLine(result.OnePlusFourCommsCostApportionmentPercentages);
            Console.WriteLine(result.OnePlusFourCommsCostApportionmentPercentages);
            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);
            var expectedJson = """
                {
                  "onePlusFourCommsCostApportionmentPercentages": {
                    "england"        : "1.23%",
                    "wales"          : "2.34%",
                    "scotland"       : "3.45%",
                    "northernIreland": "4.56%",
                    "total"          : "11.58%"
                  }
                }
                """;
            JsonTestUtils.AssertJson(expectedJson, json);
        }

        [TestMethod]
        public void CalcResultCommsCost_From_Empty()
        {
            var data = new CalcResultCommsCost();

            var result = CalcResultCommsCostJson.From(data);
            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);
            var expectedJson = """
                {
                  "onePlusFourCommsCostApportionmentPercentages": {
                    "england"        : null,
                    "wales"          : null,
                    "scotland"       : null,
                    "northernIreland": null,
                    "total"          : null
                  }
                }
                """;
            JsonTestUtils.AssertJson(expectedJson, json);
        }
    }
}
