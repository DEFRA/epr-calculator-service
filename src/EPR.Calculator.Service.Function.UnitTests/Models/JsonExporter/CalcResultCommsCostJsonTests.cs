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
        public void From_WithApportionment()
        {
            var data = TestDataHelper.GetCalcResultCommsCostReportDetail();

            var result = CalcResultCommsCostJson.From(data);
            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);
            var expectedJson = """
                {
                "onePlusFourCommsCostApportionmentPercentages": {
                    "england": null,
                    "wales": null,
                    "scotland": null,
                    "northernIreland": null,
                    "total": null
                }
                }
                """;
            JsonTestUtils.AssertJson(expectedJson, json);
        }

        [TestMethod]
        public void From_WithApportionment_AppendsPercentSignsAndDefaultsEmptyToZeroPercent()
        {
            var data = new CalcResultCommsCost
            {
                CalcResultCommsCostOnePlusFourApportionment = new CalcResultCommsCostOnePlusFourApportionment
                {
                    Name = CalcResultCommsCostBuilder.OnePlusFourApportionment,
                    England = 10,
                    Wales = 20,
                    Scotland = 30,
                    NorthernIreland = 0,
                    Total = 0
                }
            };

            var result = CalcResultCommsCostJson.From(data);

            Assert.IsNotNull(result);
            var pct = result.OnePlusFourCommsCostApportionmentPercentages;
            Assert.IsNotNull(pct);
            Assert.AreEqual("10.00%", pct.England);
            Assert.AreEqual("20.00%", pct.Wales);
            Assert.AreEqual("30.00%", pct.Scotland);
            Assert.AreEqual( "0.00%", pct.NorthernIreland);
            Assert.AreEqual( "0.00%", pct.Total);
        }

        [TestMethod]
        public void From_WithoutApportionment_ReturnsEmptyPercentagesObject()
        {
            var data = new CalcResultCommsCost();

            var result = CalcResultCommsCostJson.From(data);

            Assert.IsNotNull(result);
            var pct = result.OnePlusFourCommsCostApportionmentPercentages;
            Assert.IsNotNull(pct);
            Assert.IsNull(pct.England);
            Assert.IsNull(pct.Wales);
            Assert.IsNull(pct.Scotland);
            Assert.IsNull(pct.NorthernIreland);
            Assert.IsNull(pct.Total);
        }
    }
}
