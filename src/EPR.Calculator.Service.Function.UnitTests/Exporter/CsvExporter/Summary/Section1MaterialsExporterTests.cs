using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.UnitTests.Builder;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Summary;

[TestClass]
public class Section1MaterialsExporterTests
{
    private readonly ICalcResultSummaryPartExporter exporter = new Section1MaterialsExporter();

    [TestMethod]
    public void Section1MaterialsExporter_Export_CSV_Aluminium()
    {
        // Arrange
        var materials = TestDataHelper.GetMaterials().Where(m => m.Code == "AL").ToList();
        const bool applyModulation = false;
        var resultSummary = TestDataHelper.GetCalcResultSummary();
        var producer = resultSummary.ProducerDisposalFees.First();
        producer.ProducerDisposalFeesByMaterial =
            producer.ProducerDisposalFeesByMaterial
                .Where(kv => kv.Key == "AL")
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        var csvContent = new StringBuilder();

        // Act
        SummaryExporterTestUtils.Render(exporter, materials, applyModulation, resultSummary, csvContent);
        var result = csvContent.ToString().Split("\n").ToArray();
        Console.WriteLine(string.Join("\n", result));

        // 15 columns: section/group header spans 1 cell + 14 padding nulls + 1 trailing null = 16 elements
        var expected = new string?[][] {
            ["1 Producer Disposal Fees with Bad Debt Provision",
             null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
            ["Aluminium Breakdown",
             null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
            ["Previous Invoiced Tonnage",
             "Household Packaging Tonnage",
             "Public Bin Tonnage",
             "Total Tonnage",
             "Self Managed Consumer Waste Tonnage",
             "Net Tonnage",
             "Tonnage Change",
             "Price per Tonne",
             "Producer Disposal Fee w/o Bad Debt Provision",
             "Bad Debt Provision",
             "Producer Disposal Fee with Bad Debt Provision",
             "England with Bad Debt Provision",
             "Wales with Bad Debt Provision",
             "Scotland with Bad Debt Provision",
             "Northern Ireland with Bad Debt Provision",
             null
            ],
            ["-",
             "1000.000",
             "0.000",
             "0.000",
             "90.000",
             "910.000",
             "0.000",
             "£0.6676",
             "£607.52",
             "£36.45",
             "£643.97",
             "£348.06",
             "£78.46",
             "£156.28",
             "£61.18",
             null
            ]
        };

        CsvTestUtils.AssertCsv(expected, result);
    }

    [TestMethod]
    public void Section1MaterialsExporter_Export_CSV_Glass()
    {
        // Arrange
        var materials = TestDataHelper.GetMaterials().Where(m => m.Code == "GL").ToList();
        const bool applyModulation = false;
        var resultSummary = TestDataHelper.GetCalcResultSummary();
        var producer = resultSummary.ProducerDisposalFees.First();
        producer.ProducerDisposalFeesByMaterial =
            producer.ProducerDisposalFeesByMaterial
                .Where(kv => kv.Key == "GL")
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        var csvContent = new StringBuilder();

        // Act
        SummaryExporterTestUtils.Render(exporter, materials, applyModulation, resultSummary, csvContent);
        var result = csvContent.ToString().Split("\n").ToArray();
        Console.WriteLine(string.Join("\n", result));

        var expected = new string?[][] {
            ["1 Producer Disposal Fees with Bad Debt Provision",
             null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
            ["Glass Breakdown",
             null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
            ["Previous Invoiced Tonnage",
             "Household Packaging Tonnage",
             "Public Bin Tonnage",
             "Household Drinks Containers Tonnage - Glass",
             "Total Tonnage",
             "Self Managed Consumer Waste Tonnage",
             "Net Tonnage",
             "Tonnage Change",
             "Price per Tonne",
             "Producer Disposal Fee w/o Bad Debt Provision",
             "Bad Debt Provision",
             "Producer Disposal Fee with Bad Debt Provision",
             "England with Bad Debt Provision",
             "Wales with Bad Debt Provision",
             "Scotland with Bad Debt Provision",
             "Northern Ireland with Bad Debt Provision",
             null
            ],
            ["0.000",
             "500.000",
             "0.000",
             "220.000",
             "0.000",
             "150.000",
             "350.000",
             "0.000",
             "£6.4404",
             "£2254.14",
             "£135.25",
             "£2389.39",
             "£1291.43",
             "£291.10",
             "£579.85",
             "£227.00",
             null
            ]
        };

        CsvTestUtils.AssertCsv(expected, result);
    }
}
