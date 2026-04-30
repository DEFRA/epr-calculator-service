using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using FluentValidation;

namespace EPR.Calculator.Service.Function.Features.Billing.Contexts;

/// <summary>
///     Ensures the state of the CalculatorRun database entity is valid for billing file generation.
/// </summary>
public class BillingRunValidator : AbstractValidator<CalculatorRun>
{
    private static readonly ImmutableHashSet<BillingRunStatus> ValidBillingRunStatuses = [
        BillingRunStatus.None,
        BillingRunStatus.Starting,
        BillingRunStatus.Running,
        BillingRunStatus.Errored
    ];

    public BillingRunValidator()
    {
        RuleFor(run => run.Name)
            .NotEmpty()
            .WithMessage("Run has no name");

        RuleFor(run => run.DefaultParameterSettingMasterId)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("Run is missing DefaultParameterSettingMaster");

        RuleFor(run => run.LapcapDataMasterId)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("Run is missing LapcapDataMaster");

        RuleFor(run => run.CalculatorRunOrganisationDataMasterId)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("Run is missing OrganisationDataMaster");

        RuleFor(run => run.CalculatorRunPomDataMasterId)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("Run is missing PomDataMaster");

        RuleFor(run => run.BillingRunStatus)
            .Must(billingRunStatus => ValidBillingRunStatuses.Contains(billingRunStatus))
            .WithMessage($"Run BillingRunStatus must be one of [{string.Join(", ", ValidBillingRunStatuses)}]");
    }
}
