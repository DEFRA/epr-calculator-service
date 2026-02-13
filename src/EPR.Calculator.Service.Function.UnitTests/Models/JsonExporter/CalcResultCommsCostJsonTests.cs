namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    using System.Collections.Generic;
    using EPR.Calculator.Service.Function.Builder.CommsCost;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Models.JsonExporter;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResultCommsCostJsonTests
    {
        [TestMethod]
        public void From_WithApportionment_AppendsPercentSignsAndDefaultsEmptyToZeroPercent()
        {
            var data = new CalcResultCommsCost
            {
                CalcResultCommsCostOnePlusFourApportionment = new List<CalcResultCommsCostOnePlusFourApportionment>
                {
                    new CalcResultCommsCostOnePlusFourApportionment
                    {
                        Name = CalcResultCommsCostBuilder.OnePlusFourApportionment,
                        England = "10",
                        Wales = "20%",
                        Scotland = " 30 ",
                        NorthernIreland = "",
                        Total = string.Empty
                    }
                }
            };

            var result = CalcResultCommsCostJson.From(data);

            Assert.IsNotNull(result);
            var pct = result.OnePlusFourCommsCostApportionmentPercentages;
            Assert.IsNotNull(pct);
            Assert.AreEqual("10%", pct.England);
            Assert.AreEqual("20%", pct.Wales);
            Assert.AreEqual("30%", pct.Scotland);
            Assert.AreEqual("0.00%", pct.NorthernIreland);
            Assert.AreEqual("0.00%", pct.Total);
        }

        [TestMethod]
        public void From_WithoutApportionment_ReturnsEmptyPercentagesObject()
        {
            var data = new CalcResultCommsCost
            {
                CalcResultCommsCostOnePlusFourApportionment = new List<CalcResultCommsCostOnePlusFourApportionment>()
            };

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
