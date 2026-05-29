using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.UnitTests.Builder;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Summary;

[TestClass]
public class Section2aCommsExporterTests
{
    private readonly ICalcResultSummaryPartExporter exporter = new Section2aCommsExporter();

    [TestMethod]
    public void Section2aCommsExporter_Export_CSV()
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

        var expected = new string?[][] {
            new string?[8],
            ["Summary of Fee for Comms Costs - by Material", null, null, null, null, null, null, null],
            ["2a Total Producer Fee for Comms Costs - by Material w/o Bad Debt provision",
             "Total Bad Debt Provision",
             "2a Total Producer Fee for Comms Costs - by Material with Bad Debt provision",
             "England Total with Bad Debt provision",
             "Wales Total with Bad Debt provision",
             "Scotland Total with Bad Debt provision",
             "Northern Ireland Total with Bad Debt provision",
             null],
            ["£1290.78", "£77.45", "£1368.22", "£718.23", "£181.27", "£332.85", "£135.88", null]
        };

        CsvTestUtils.AssertCsv(expected, result);
    }
}
