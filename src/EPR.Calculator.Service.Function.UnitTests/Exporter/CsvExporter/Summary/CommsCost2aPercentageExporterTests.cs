using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.UnitTests.Builder;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Summary;

[TestClass]
public class CommsCost2aPercentageExporterTests
{
    private readonly ICalcResultSummaryPartExporter exporter = new CommsCost2aPercentageExporter();

    [TestMethod]
    public void CommsCost2aPercentageExporterTests_Export_CSV()
    {
        // Arrange
        var materials = TestDataHelper.GetMaterials();
        const bool applyModulation = false;
        var resultSummary = TestDataHelper.GetCalcResultSummary();
        var csvContent = new StringBuilder();

        // Act
        SummaryExporterTestUtils.Render(exporter, materials, applyModulation, resultSummary, csvContent);
        var result = csvContent.ToString().Split("\n").ToArray();
        Console.WriteLine(string.Join("\n", result));

        // 1 column: default section/group headers → [null, null] (1 comma each)
        var expected = new string?[][] {
            [null, null],
            [null, null],
            ["Percentage of Producer Tonnage vs All Producers", null],
            ["5.67415285%", null]
        };

        CsvTestUtils.AssertCsv(expected, result);
    }
}
