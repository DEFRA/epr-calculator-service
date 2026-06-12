using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using FluentValidation;

namespace EPR.Calculator.Service.Function.Features.BillingRun.Contexts;

/// <summary>
///     Ensures the state of the CalculatorRun database entity is valid for billing file generation.
/// </summary>
public class BillingRunValidator : AbstractValidator<API.Data.DataModels.CalculatorRun>
{
    private static readonly ImmutableHashSet<int> ValidClassifications =
    [
        RunClassificationStatusIds.INITIALRUNID,
        RunClassificationStatusIds.INTERIMRECALCULATIONRUNID,
        RunClassificationStatusIds.FINALRECALCULATIONRUNID,
        RunClassificationStatusIds.FINALRUNID
    ];

    public BillingRunValidator()
    {
        RuleFor(run => run.Name)
            .NotEmpty()
            .WithMessage("Run has no name");

        RuleFor(run => run.DefaultParameterSettingMasterId)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage($"Run is missing {nameof(DefaultParameterSettingMaster)}");

        RuleFor(run => run.LapcapDataMasterId)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage($"Run is missing {nameof(LapcapDataMaster)}");

        RuleFor(run => run.CalculatorRunOrganisationDataMasterId)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage($"Run is missing {nameof(CalculatorRunOrganisationDataMaster)}");

        RuleFor(run => run.CalculatorRunPomDataMasterId)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage($"Run is missing {nameof(CalculatorRunPomDataMaster)}");

        RuleFor(run => run.CalculatorRunClassificationId)
            .Must(classification => ValidClassifications.Contains(classification))
            .WithMessage($"Run classification must be one of [{string.Join(", ", ValidClassifications)}]");

        RuleFor(run => run.BillingRunStatus)
            .Equal(BillingRunStatus.Running)
            .WithMessage("Run BillingRunStatus must be running");
    }
}
