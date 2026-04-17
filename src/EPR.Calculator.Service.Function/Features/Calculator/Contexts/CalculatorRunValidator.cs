using System.Collections.Immutable;
using EPR.Calculator.API.Data.DataModels;
using FluentValidation;

namespace EPR.Calculator.Service.Function.Features.Calculator.Contexts;

public class CalculatorRunValidator : AbstractValidator<CalculatorRun>
{
    public CalculatorRunValidator()
    {
        RuleFor(run => run.Name)
            .NotEmpty()
            .WithMessage("Run has no name");

        RuleFor(run => run.DefaultParameterSettingMasterId)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("Run is missing ParameterSettingMaster");

        RuleFor(run => run.LapcapDataMasterId)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("Run is missing LapcapDataMaster");

        RuleFor(run => run.CalculatorRunOrganisationDataMasterId)
            .Null()
            .WithMessage("Run already has OrganisationDataMaster associated");

        RuleFor(run => run.CalculatorRunPomDataMasterId)
            .Null()
            .WithMessage("Run already has PomDataMaster associated");

        ImmutableHashSet<int> validStates = [RunClassificationStatusIds.INTHEQUEUEID, RunClassificationStatusIds.RUNNINGID];

        RuleFor(run => run.CalculatorRunClassificationId)
            .Must(statusId => validStates.Contains(statusId))
            .WithMessage("Run classification must be INTHEQUEUE/RUNNING");
    }
}