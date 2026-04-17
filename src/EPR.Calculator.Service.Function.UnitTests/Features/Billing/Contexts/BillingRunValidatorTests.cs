using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Features.Billing.Contexts;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using FluentValidation.TestHelper;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Billing.Contexts;

[TestCategory(TestCategories.BillingRuns)]
[TestClass]
public class BillingRunValidatorTests
{
    private BillingRunValidator _sut = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _sut = new BillingRunValidator();
    }

    [TestMethod]
    public void Should_not_error_when_run_is_valid()
    {
        var run = new CalculatorRun
        {
            Name = "TestRun",
            DefaultParameterSettingMasterId = 1,
            LapcapDataMasterId = 1,
            CalculatorRunOrganisationDataMasterId = 1,
            CalculatorRunPomDataMasterId = 1,
            IsBillingFileGenerating = true
        };

        var result = _sut.TestValidate(run);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [TestMethod]
    public void Should_error_for_empty_Name(string? name)
    {
        var run = new CalculatorRun
        {
            Name = name!,
            DefaultParameterSettingMasterId = 1,
            LapcapDataMasterId = 1,
            CalculatorRunOrganisationDataMasterId = 1,
            CalculatorRunPomDataMasterId = 1,
            IsBillingFileGenerating = true
        };

        var result = _sut.TestValidate(run);

        result.ShouldHaveValidationErrorFor(r => r.Name);
    }

    [DataRow(null)]
    [DataRow(0)]
    [TestMethod]
    public void Should_error_for_empty_DefaultParameterSettingMasterId(int? id)
    {
        var run = new CalculatorRun
        {
            Name = "TestRun",
            DefaultParameterSettingMasterId = id,
            LapcapDataMasterId = 1,
            CalculatorRunOrganisationDataMasterId = 1,
            CalculatorRunPomDataMasterId = 1,
            IsBillingFileGenerating = true
        };

        var result = _sut.TestValidate(run);

        result.ShouldHaveValidationErrorFor(r => r.DefaultParameterSettingMasterId);
    }

    [DataRow(null)]
    [DataRow(0)]
    [TestMethod]
    public void Should_error_for_empty_LapcapDataMasterId(int? id)
    {
        var run = new CalculatorRun
        {
            Name = "TestRun",
            DefaultParameterSettingMasterId = 1,
            LapcapDataMasterId = id,
            CalculatorRunOrganisationDataMasterId = 1,
            CalculatorRunPomDataMasterId = 1,
            IsBillingFileGenerating = true
        };

        var result = _sut.TestValidate(run);

        result.ShouldHaveValidationErrorFor(r => r.LapcapDataMasterId);
    }

    [DataRow(null)]
    [DataRow(0)]
    [TestMethod]
    public void Should_error_for_empty_CalculatorRunOrganisationDataMasterId(int? id)
    {
        var run = new CalculatorRun
        {
            Name = "TestRun",
            DefaultParameterSettingMasterId = 1,
            LapcapDataMasterId = id,
            CalculatorRunOrganisationDataMasterId = id,
            CalculatorRunPomDataMasterId = 1,
            IsBillingFileGenerating = true
        };

        var result = _sut.TestValidate(run);

        result.ShouldHaveValidationErrorFor(r => r.CalculatorRunOrganisationDataMasterId);
    }

    [DataRow(null)]
    [DataRow(0)]
    [TestMethod]
    public void Should_error_for_empty_CalculatorRunPomDataMasterId(int? id)
    {
        var run = new CalculatorRun
        {
            Name = "TestRun",
            DefaultParameterSettingMasterId = 1,
            LapcapDataMasterId = id,
            CalculatorRunOrganisationDataMasterId = 1,
            CalculatorRunPomDataMasterId = id,
            IsBillingFileGenerating = true
        };

        var result = _sut.TestValidate(run);

        result.ShouldHaveValidationErrorFor(r => r.CalculatorRunPomDataMasterId);
    }

    [DataRow(null)]
    [DataRow(false)]
    [TestMethod]
    public void Should_error_when_billing_status_is_incorrect(bool? status)
    {
        var run = new CalculatorRun
        {
            Name = "TestRun",
            DefaultParameterSettingMasterId = 1,
            CalculatorRunOrganisationDataMasterId = 1,
            CalculatorRunPomDataMasterId = 1,
            LapcapDataMasterId = 1,
            IsBillingFileGenerating = false
        };

        var result = _sut.TestValidate(run);

        result.ShouldHaveValidationErrorFor(r => r.IsBillingFileGenerating);
    }
}