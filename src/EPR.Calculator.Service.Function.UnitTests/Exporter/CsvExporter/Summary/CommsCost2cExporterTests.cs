using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.UnitTests.Builder;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Summary;

[TestClass]
public class CommsCost2cExporterTests
{
    private readonly ICalcResultSummaryPartExporter exporter = new CommsCost2cExporter();

    [TestMethod]
    public void CommsCost2cExporter_Export_CSV()
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

        // 7 columns: 3 content cells + 4 padding nulls + 1 trailing null = 8 elements per header row
        var expected = new string?[][] {
            ["2c Comms Costs - by Country w/o Bad Debt provision",
             "Bad Debt provision",
             "2c Comms Costs - by Country with Bad Debt provision",
             null,
             null,
             null,
             null,
             null
            ],
            ["£1339.10", "£80.35", "£1419.45", null, null, null, null, null],
            ["2c Total Producer Fee for Comms Costs - by Country In proportion to Producer Tonnage w/o Bad Debt provision",
             "Bad Debt Provision for 2c",
             "2c Total Producer Fee for Comms Costs - by Country In proportion to Producer Tonnage with Bad Debt provision",
             "England Total with Bad Debt provision",
             "Wales Total with Bad Debt provision",
             "Scotland Total with Bad Debt provision",
             "Northern Ireland Total with Bad Debt provision",
             null],
            ["£1339.10", "£80.35", "£1419.45", "£607.47", "£300.73", "£360.88", "£150.37", null]
        };

        CsvTestUtils.AssertCsv(expected, result);
    }
}
