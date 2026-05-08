using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Features.Billing.Contexts;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Features.Common;

namespace EPR.Calculator.Service.Function.UnitTests.TestHelpers.Extensions;

public static class RunContextTestExtensions
{
    public static CalculatorRun ToEntity(this RunContext runContext, Action<CalculatorRun>? configure = null)
    {
        if(runContext is CalculatorRunContext calculatorRunContext)
            return calculatorRunContext.ToEntity(configure);

        if(runContext is BillingRunContext billingRunContext)
            return billingRunContext.ToEntity(configure);

        throw new ArgumentException("Invalid run context type", nameof(runContext));
    }

    public static CalculatorRun ToEntity(this CalculatorRunContext runContext, Action<CalculatorRun>? configure = null)
    {
        var run = new CalculatorRun
        {
            Id = runContext.RunId,
            RelativeYear = runContext.RelativeYear,
            Name = runContext.RunName,
            CreatedBy = runContext.User,
            CreatedAt = runContext.ProcessingStartedAt.AddHours(-1).UtcDateTime,
            Classification = RunClassification.Running,
            BillingRunStatus = BillingRunStatus.None,
            BillingRunStartedAt = null,
            DefaultParameterSettingMasterId = 1,
            LapcapDataMasterId = 1,
            CalculatorRunPomDataMasterId = null,
            CalculatorRunOrganisationDataMasterId = null
        };

        configure?.Invoke(run);
        return run;
    }

    public static CalculatorRun ToEntity(this BillingRunContext runContext, Action<CalculatorRun>? configure = null)
    {
        var run = new CalculatorRun
        {
            Id = runContext.RunId,
            RelativeYear = runContext.RelativeYear,
            Name = runContext.RunName,
            CreatedBy = runContext.User,
            CreatedAt = runContext.ProcessingStartedAt.AddHours(-1).UtcDateTime,
            Classification = RunClassification.Unclassified,
            BillingRunStatus = BillingRunStatus.Running,
            BillingRunStartedAt = runContext.ProcessingStartedAt.AddHours(-1).UtcDateTime,
            DefaultParameterSettingMasterId = 1,
            LapcapDataMasterId = 1,
            CalculatorRunPomDataMasterId = 1,
            CalculatorRunOrganisationDataMasterId = 1
        };

        configure?.Invoke(run);
        return run;
    }
}
