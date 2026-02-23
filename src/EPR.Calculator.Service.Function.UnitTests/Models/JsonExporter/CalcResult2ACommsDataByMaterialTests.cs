namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    using System.Linq;
    using EPR.Calculator.Service.Function.Builder.CommsCost;

    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Models.JsonExporter;
    using EPR.Calculator.Service.Function.UnitTests.Builder;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResult2ACommsDataByMaterialTests
    {
        [TestMethod]
        public void From_MapsMaterialsAndTotal()
        {
            // Arrange
            var comms = GetCalcResultCommsCostCommsCostByMaterials();

            // Act
            var result = CalcResult2ACommsDataByMaterial.From(comms);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.CalcResult2aCommsDataDetails.Any());

            var first = result.CalcResult2aCommsDataDetails.First();
            Assert.IsNotNull(first);
            Assert.AreEqual("Aluminium", first.MaterialName);
            Assert.AreEqual("£0.4200", first.CommsCostByMaterialPricePerTonne);

            Assert.IsNotNull(result.CalcResult2aCommsDataDetailsTotal);
        }

        private static List<CalcResultCommsCostCommsCostByMaterial> GetCalcResultCommsCostCommsCostByMaterials()
        {
            return new List<CalcResultCommsCostCommsCostByMaterial>
            {
                new CalcResultCommsCostCommsCostByMaterial
                {
                    CommsCostByMaterialPricePerTonne = "0.42",
                    CommsCostByMaterialPricePerTonneValue = 0.42m,
                    Name = "Aluminium",
                },
                new CalcResultCommsCostCommsCostByMaterial
                {
                    CommsCostByMaterialPricePerTonne = "0.3",
                    CommsCostByMaterialPricePerTonneValue = 0.3m,
                    Name = "Glass",
                },
                new CalcResultCommsCostCommsCostByMaterial
                {
                    CommsCostByMaterialPricePerTonne = "0.51",
                    CommsCostByMaterialPricePerTonneValue = 0.51m,
                    Name = "Total",
                },
            };
        }
    }
}
