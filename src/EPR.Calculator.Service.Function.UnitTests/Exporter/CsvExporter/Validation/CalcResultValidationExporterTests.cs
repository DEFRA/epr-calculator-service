using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Validation;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using FluentValidation;
using FluentValidation.Results;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Validation;

[TestClass]
public class CalcResultValidationExporterTests : TestsFor<CalcResultValidationExporter>
{
    private CalcResult calcResult = new()
    {
        CalcResultDetail = null!,
        CalcResultLapcapData = null!,
        CalcResultCommsCostReportDetail = null!,
        CalcResultLateReportingTonnageData = null!,
        CalcResultParameterOtherCost = null!,
        CalcResultOnePlusFourApportionment = null!,
        CalcResultLaDisposalCostData = null!,
        CalcResultPartialObligations = null!,
        CalcResultProjectedProducers = null,
        CalcResultScaledupProducers = null,
        CalcResultCancelledProducers = null!,
        ProducerFees = null!,
        Smcw = null!,
        CalcResultModulation = null,
        CalcResultErrorReports = null!
    };

    private Mock<IValidator<CalcResult>> validator = null!;

    protected override void TestInitialize() => validator = fixture.Freeze<Mock<IValidator<CalcResult>>>();

    [TestMethod]
    public void Should_not_add_warnings()
    {
        // Arrange
        var csvContent = new StringBuilder("existing content");

        // Act
        testSubject.ExportWarnings(calcResult, csvContent);

        // Assert
        csvContent.ToString().ShouldBe("existing content");
    }

    [TestMethod]
    public void Should_add_warnings()
    {
        // Arrange
        var csvContent = new StringBuilder();

        validator
            .Setup(x => x.Validate(It.IsAny<CalcResult>()))
            .Returns(new ValidationResult([
                new ValidationFailure("Test1", "First warning") { Severity = Severity.Warning },
                new ValidationFailure("Test2", "Second warning") { Severity = Severity.Warning },
                new ValidationFailure("Test3", "Third warning") { Severity = Severity.Warning }
            ]));

        // Act
        testSubject.ExportWarnings(calcResult, csvContent);

        // Assert
        var result = csvContent.ToString().ReplaceLineEndings("\n").Split("\n").Select(s => s.TrimEnd(',')).ToArray();
        var expected = new string[][]
        {
            [],
            [],
            ["Warning"],
            ["First warning"],
            ["Second warning"],
            ["Third warning"],
            []
        };

        CsvTestUtils.AssertCsv(expected, result);
    }
}
