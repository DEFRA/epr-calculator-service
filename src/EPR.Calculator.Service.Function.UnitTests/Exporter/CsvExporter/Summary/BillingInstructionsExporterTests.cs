using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.UnitTests.Builder;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Summary;

[TestClass]
public class BillingInstructionsExporterTests
{
    private readonly ICalcResultSummaryPartExporter exporter = new BillingInstructionsExporter();

    [TestMethod]
    public void BillingInstructionsExporter_Export_CSV()
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

        // 10 columns: section header spans 1 cell + 9 padding nulls + 1 trailing null = 11 elements
        //             default group header → 11 nulls (10 commas)
        var expected = new string?[][] {
            ["Calculation of Suggested Billing Instructions and Invoice Amounts",
             null, null, null, null, null, null, null, null, null, null],
            new string?[11],
            ["Current Year Invoiced Total To Date",
             "Tonnage Change Since Last Invoice",
             "Liability Difference (Calc vs Prev)",
             "Material £ Threshold Breached",
             "Tonnage £ Threshold Breached (if tonnage changed)",
             "% Liability Difference (Calc vs Prev)",
             "Material % Threshold Breached",
             "Tonnage % Threshold Breached (if tonnage changed)",
             "Suggested Billing Instruction",
             "Suggested Invoice Amount",
             null],
            ["£1250.89", "Tonnage Changed", "£580.73", "", "", "123.45%", "", "", "", "£4039.00", null]
        };

        CsvTestUtils.AssertCsv(expected, result);
    }
}
