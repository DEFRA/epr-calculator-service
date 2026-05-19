using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Function.Models;
using System.Text.Json.Nodes;

namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    [TestClass]
    public class CalcResultModulationResultsJsonTests
    {
        [TestMethod]
        public void From_WithValidData_MapsAndSerializesCorrectly()
        {
            var result = CalcResultModulationResults.From(new ModulationResult
            {
                RedFactor = 1.25m,
                GreenFactor = 0.75m,
                MaterialModulation = new Dictionary<MaterialDetail, MaterialModulation>()
            });

            var actualJson = JsonNode.Parse(JsonSerializer.Serialize(result));

            var expectedJson = JsonNode.Parse("""
            {
                "redFactor": 1.25,
                "greenDiscountFactor": 0.75
            }
            """);

            Assert.IsTrue(JsonNode.DeepEquals(expectedJson, actualJson));
        }
    }
}